using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class PlayerInteract : MonoBehaviour
    {

        [SerializeField] private Camera playerCamera;
        [SerializeField] private GameObject InteractUI;
        [SerializeField] private float interactDistance = 3f;

        private void Start()
        {
            InteractUI.SetActive(false);
        }


        private void Update()
        {

            Ray ray = playerCamera.ScreenPointToRay(
                    new Vector3(Screen.width / 2f,
                                Screen.height / 2f,
                                0f));

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                Interactable interactable =
                    hit.collider.GetComponent<Interactable>();

                if (interactable != null)
                {
                    InteractUI.SetActive(true);
                } 
                else
                {
                    InteractUI.SetActive(false);
                }
            } else
            {
                InteractUI.SetActive(false);
            }


            if (Input.GetKeyDown(KeyCode.E))
            {
                Ray r = playerCamera.ScreenPointToRay(
                    new Vector3(Screen.width / 2f,
                                Screen.height / 2f,
                                0f));

                if (Physics.Raycast(r, out RaycastHit h, interactDistance))
                {
                    Interactable interactable =
                        h.collider.GetComponent<Interactable>();

                    if (interactable != null)
                    {
                        interactable.Interact();
                    }
                }
            }
        }

    }
}