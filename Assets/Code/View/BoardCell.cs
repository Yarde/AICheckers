using UnityEngine;

namespace Code.View
{
    public class BoardCell : MonoBehaviour
    {
        [SerializeField] private bool interactive;
        private Animation _animation;

        private void Awake()
        {
            if (interactive)
            {
                _animation = GetComponent<Animation>();
            }
        }

        public void Highlight()
        {
            if (interactive)
            {
                _animation.Play();
            }
        }
    }
}