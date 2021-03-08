using UnityEngine;

namespace Hadal.AI
{
    public class AgentTravel : MonoBehaviour
    {
        public GameObject[] spots;
        public GameObject self;
        public float speed;
        public Vector3 direction;
        public Vector3 destination;
        int rando;


        // Start is called before the first frame update
        void Start()
        {
            ChooseSpot();
            MoveAroundSpots();
        }

        // Update is called once per frame
        void Update()
        {
            MoveAroundSpots();
        }

        private void OnTriggerEnter(Collider other)
        {
            for (int i = 0; i < spots.Length; i++)
            {
                spots[i].SetActive(true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            MoveAroundSpots();
        }

        private void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < spots.Length; i++)
            {
                spots[i].SetActive(false);
            }
        }

        void MoveAroundSpots()
        {
            direction = (destination - self.transform.position).normalized;

            self.transform.position += direction * speed * Time.deltaTime;

            if (Mathf.Abs(Vector3.Distance(self.transform.position, destination)) < 0.1)
            {
                ChooseSpot();
            }
        }

        void ChooseSpot()
        {
            rando = Random.Range(0, spots.Length);
            destination = spots[rando].transform.position;

            Debug.Log(rando);
        }
    }
}