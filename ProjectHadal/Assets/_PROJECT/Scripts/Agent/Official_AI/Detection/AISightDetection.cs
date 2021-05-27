using UnityEngine;

namespace Hadal.AI
{
    public class AISightDetection : MonoBehaviour, ILeviathanComponent
    {
        AIBrain _brain;

        [SerializeField] float raycastRadius; // width of our line of sight (x-axis and y-axis)
        RaycastHit hitInfo;
        bool detectedPlayer;
        bool LeftRightZ = true;
        float EyeScanZ;
        [SerializeField] float viewDistance; // depth of our line of sight (z-axis)
        public bool DetectedAPlayer => detectedPlayer;

        public UpdateMode LeviathanUpdateMode => UpdateMode.PreUpdate;

        public void Initialise(AIBrain brain)
        {
            _brain = brain;
        }

        public void DoUpdate(in float deltaTime)
        {
            CheckPlayerInLOS();
        }

        public void DoFixedUpdate(in float fixedDeltaTime)
        {
        }

        public void DoLateUpdate(in float deltaTime)
        {
            detectedPlayer = false;
        }

        void CheckPlayerInLOS()
        {
            //!maybe can add up down
            if (LeftRightZ)
            {
                if (EyeScanZ < 30)
                {
                    EyeScanZ += 100 * Time.deltaTime;
                }
                else
                {
                    LeftRightZ = false;
                }
            }
            else
            {
                if (EyeScanZ > -30)
                {
                    EyeScanZ -= 100 * Time.deltaTime;
                }
                else
                {
                    LeftRightZ = true;
                }
            }

            transform.localEulerAngles = new Vector3(0, EyeScanZ);

            detectedPlayer = Physics.SphereCast(transform.position, raycastRadius / 2, transform.forward, out hitInfo, viewDistance);

            if (detectedPlayer)
            {
                if (hitInfo.transform.CompareTag("Player"))
                {
                    //! Ask brain to do biting here
                    Debug.Log(hitInfo.collider.gameObject.name + "Limpeh see u");
                }
                else
                {
                    //! Continue swim around?
                    Debug.Log("I CANT DETECT ANY YUMYUM");
                }
            }

        }

        //Touching
        void OnTriggerEnter(Collider other)
        {

        }

        void OnDrawGizmos()
        {
            if (detectedPlayer)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(new Vector3(0f, 0f, viewDistance), raycastRadius / 2);
        }
    }
}
