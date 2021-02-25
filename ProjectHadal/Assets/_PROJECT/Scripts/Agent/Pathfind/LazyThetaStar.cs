// #define _PATHDEBUGEVENT_
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using FlyAgent.Utilities;

namespace FlyAgent.Navigation
{
	/// <summary>
	/// I think IPF is shortform for IPathFind. It represents a vertex (or an octree node) that is used in pathfinding.
	/// <br/>
	/// See: <see cref="Octree.Node" />
	/// </summary>
	public interface IPFVertex<IPF> where IPF : IPFVertex<IPF>, IEquatable<IPF>
	{
		/// <summary> Checks if there is a line of sight between this vertex and the other vertex. </summary>
		bool LineOfSight(IPF other);

		/// <summary> Calculates and returns the cost to travel from this vertex to the other vertex. </summary>
		float Cost(IPF other, float sizeRef);

		/// <summary> Returns a an IEnumrable (usually a List) of the vertex neighbours of this vertex. </summary>
		IEnumerable<IPF> Neighbours();
	}

	public static class LazyThetaStar<IPF> where IPF : IPFVertex<IPF>, IEquatable<IPF>
	{
		/// <summary> Path Finder class that implements the Lazy Theta Star algorithm. </summary>
		public class PathFinder : IDisposable
		{
			/// <summary> A cap iteration count to avoid infinite loop accidents. </summary>
			public int maxIterations = 200;

			/// <summary> The heuristic from A*. In this example, it is set at a fixed value. </summary>
			public float heuristicWeight = 1.5f;

			/// <summary> Contains information about a pathfinding vertex and its cached cost value. </summary>
			struct PathPackage : IComparable<PathPackage>
			{
				public PathPackage(IPF val, float gph)
				{
					value = val;
                    this.gph = gph;
				}
				public IPF value;
				public float gph;

				/// <summary> This is purely meant to be used for the PriorityQueue of the <see cref="open"/> list. This will
				/// determine which nodes will be placed on priority based on the cached cost value (<see cref="gph"/>). For
				/// the current implementation, the lower cost == higher priority. </summary>
				public int CompareTo(PathPackage other)
				{
					if (gph < other.gph)
					{
						return -1;
					}
					if (gph > other.gph)
						return 1;
					return 0;
				}
			}
			
			/// <summary> Open list is using a priority queue. This priority queue will always dequeue the highest priority item in
			/// the list. See <see cref="PathPackage.CompareTo"/>. </summary>
			PriorityQueue<PathPackage> open = new PriorityQueue<PathPackage>();
			
			/// <summary> Closed list where all visited vertices are placed. It is a hashset because we just need to check whether a
			/// vertex is inside this list or not. See: <see href="https://www.geeksforgeeks.org/hashset-in-c-sharp-with-examples/amp/"/> </summary>
			HashSet<IPF> close = new HashSet<IPF>();
			
			/// <summary> Keeps track of every parent of each computed vertex. The <see cref="start"/> is always the parent of itself. </summary>
			Dictionary<IPF,IPF> parent = new Dictionary<IPF, IPF>();
			
			/// <summary> Stores the cost for every single vertex that was ever computed by the algorithm :) </summary>
			Dictionary<IPF,float> g = new Dictionary<IPF, float>();
			
			/// <summary> Start vertex where the pathfinding search will begin. </summary>
			public IPF start { get; private set; }
			/// <summary> End vertex where the pathfinding search will end. </summary>
			public IPF end { get; private set; }
			
			/// <summary> Current iteration count in pathfinding loop. See: <see cref="maxIterations"/>. </summary>
			int iteration = 0;

			/// <summary> Task state for the internal C# state machine when using asynchronuous functions. </summary>
			private Task m_Task = null;

			/// <summary> SqrMagnitude size of the boundary box of the agent so pathfinding will account for its size.
			/// See: <see cref=""/>. </summary>
			private float m_SizeRef;

			/// <summary> Constructor with some default values. Inserts the <see cref="open"/> list automatically. </summary>
			public PathFinder(IPF start, IPF end, float sizeRef, float heuristicWeight = 1.0f, int maxIterations = 200)
			{
				this.start = start;
				this.end = end;
				this.heuristicWeight = heuristicWeight;
				this.maxIterations = maxIterations;
				m_SizeRef = sizeRef;
				parent[start] = start;
				g[start] = 0;
				open.Enqueue(new PathPackage(start, g[start] + this.heuristicWeight * start.Cost(end, sizeRef)));
			}

			/// <summary> No need use </summary>
			public event Action<IPF> DebugOnExpanded;
			
			/// <summary>
            /// Original text: the code here is basicly a translation of psuedo-code from http://aigamedev.com/wp-content/blogs.dir/5/files/2013/07/fig53-full.png
            /// <br/>
            /// This is the actual function executing the Lazy algorithm (non-async).
            /// </summary>
			private bool InternalIterate()
			{
				while (open.Count() != 0)
				{
					var s = open.Dequeue().value;
#if _PATHDEBUGEVENT_
					if (DebugOnExpanded != null)
						DebugOnExpanded(s);
#endif
					if (close.Contains(s))
						continue;
					SetVertex(s);
					close.Add(s);
					if (iteration++ > maxIterations || s.Equals(end))
					{
						return true;
					}
					foreach (var sp in s.Neighbours())
					{
						if (!close.Contains(sp))
						{
							if (!g.ContainsKey(sp))
							{
								g[sp] = Mathf.Infinity;
								parent.Remove(sp);
							}
							UpdateVertex(s, sp);
						}
					}
				}
				return false;
			}

			/// <summary>
            /// Unity coroutine that finds the paths between <see cref="start"/> and <see cref="end"/>.
            /// <br/>
            /// It first runs through the Lazy algorithm until it stops. Then it tries to backtrack from the end to the start (just like
            /// in A*).
            /// </summary>
			public IEnumerable QuickFind()
			{
				while (!InternalIterate())
					;

				List<IPF> res = new List<IPF>(100);
				IPF temp = close
					.OrderBy(v => v.Cost(end, m_SizeRef))
					.FirstOrDefault();
				if (temp == null)
					return null;
				while (!parent[temp].Equals(temp))
				{
					res.Add(temp);
					temp = parent[temp];
				}
				res.Reverse();
				return res;
			}

			/// <summary> Calls the async version of the find algorithm implementation. </summary>
			public void AsyncFind(Action<List<IPF>> callback)
			{
				m_Task = _AsyncFind(callback);
			}

			/// <summary> Async implementation of Lazy algorithm. The person mentioned this is 99% the same as the non-async version
            /// but I am not even sure why lol. See: <see cref="AsyncFind"/>.
            /// </summary>
			private async Task _AsyncFind(Action<List<IPF>> callback)
			{
				// 99% same as QuickFind() + InternalIterate;
				while (open.Count() != 0)
				{
					var s = open.Dequeue().value;
					if (close.Contains(s))
						continue;
					SetVertex(s);
					close.Add(s);
					if (iteration++ > maxIterations || s.Equals(end))
					{
						break;
					}
					foreach (var sp in s.Neighbours())
					{
						if (iteration % 10 == 0)
							await Task.Delay(1); // since Delay(0)/(null) will result in dead loop.

						if (!close.Contains(sp))
						{
							if (!g.ContainsKey(sp))
							{
								g[sp] = Mathf.Infinity;
								parent.Remove(sp);
							}
							UpdateVertex(s, sp);
						}
					}
				}

				List<IPF> res = new List<IPF>(100);
				IPF temp = close
					.OrderBy(v => v.Cost(end, m_SizeRef))
					.FirstOrDefault();
				if (temp == null)
				{
					callback(res);
					return;
				}
				while (!parent[temp].Equals(temp))
				{
					res.Add(temp);
					temp = parent[temp];
				}
				res.Reverse();
				callback(res);
			}

			/// <summary> 
            /// Records the cost of <paramref name="sp"/> before computing it. Then if the newly computed cost is smaller than the recorded
            /// cost, <paramref name="sp"/> will be added as a new vertex in the <see cref="open"/> list.
            /// </summary>
            /// <param name="s">The main vertex being compared.</param>
            /// <param name="sp">A neighbour of the main vertex.</param>
			void UpdateVertex(IPF s, IPF sp)
			{
				var gold = g[sp];
				ComputeCost(s, sp);
				if (g[sp] < gold)
				{
					open.Enqueue(new PathPackage(sp, g[sp] + heuristicWeight * sp.Cost(end, m_SizeRef)));
				}
			}

			/// <summary>
            /// Note: This is Path 2 in Lazy Theta Star
            /// <br/>
            /// Firstly calculates the cost by:
            /// <br/>
            /// [Parent of <paramref name="s"/>] + [Parent of <paramref name="s"/> + Cost to <paramref name="sp"/>].
            /// <br/>
            /// If the resultant cost is smaller than the original cost of <paramref name="sp"/> (originally Mathf.Infinity), <paramref name="s"/>
            /// will become its parent + update the cached cost respectively.
            /// </summary>
            /// <param name="s">The main vertex being compared.</param>
            /// <param name="sp">A neighbour of the main vertex.</param>
			void ComputeCost(IPF s, IPF sp)
			{
				if (g[parent[s]] + parent[s].Cost(sp, m_SizeRef) < g[sp])
				{
					parent[sp] = parent[s];
					g[sp] = g[parent[s]] + parent[s].Cost(sp, m_SizeRef);
				}
			}

			/// <summary>
            /// Note: This is Path 1 in Lazy Theta Star
            /// <br/>
            /// When <paramref name="s"/> has no line of sight of its parent, it will execute Path 1. It will check whether if any of <paramref name="s"/>'s
            /// neighbours are part of the <see cref="close"/> list and select the one with the lowest cost, if any. If any, the lowest cost
            /// vertex will become the parent of <paramref name="s"/>, and cost in <see cref="g"/> will be updated accordingly with the new
            /// parent.
            /// </summary>
			void SetVertex(IPF s)
			{
				if (!parent[s].LineOfSight(s))
				{
					var temp = s
						.Neighbours()
						.Intersect(close)
						.Select(sp => new { sp = sp, gpc = g[sp] + sp.Cost(s, m_SizeRef) })
						.OrderBy(sppair => sppair.gpc)
						.FirstOrDefault();
					if (temp == null)
						return;
					parent[s] = temp.sp;
					g[s] = temp.gpc;
					;
				}
			}

			#region IDisposable Support
			private bool IsDisposed = false; // To detect redundant calls

			/// <summary>
            /// Dispose function. So far, it is only called in deconstructor. Because this is in the garbage collection area, it could
            /// cause performance issues if not handled correctly.
            /// </summary>
			protected virtual void Dispose(bool disposing)
			{
				if (!IsDisposed)
				{
					if (disposing)
					{
						g.Clear();
						parent.Clear();
						close.Clear();
						// TODO: dispose managed state (managed objects).
					}

					// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
					// TODO: set large fields to null.
					if (m_Task != null)
						m_Task.Dispose();
					// m_Task = null;
					
					IsDisposed = true;
				}
			}

			// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
			~PathFinder()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose(false);
			}

			// This code added to correctly implement the disposable pattern.
			public void Dispose()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose(true);
				// TODO: uncomment the following line if the finalizer is overridden above.
				// GC.SuppressFinalize(this);
			}
			#endregion
		}

		/// <summary>
        // /// Quick way to get a path finding object. It gets called here <see cref="FlyAgent.Agents.Pilot.FindPathBetween(Octree.Node, Octree.Node)"/>.
        /// </summary>
		public static PathFinder FindPath(IPF start, IPF end, float sizeRef)
		{
			return new PathFinder(start, end, sizeRef);
		}
	}
}