using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Code.View
{
    public class BoardCell : MonoBehaviour
    {
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private Color moveColor;
        [SerializeField] private Color attackColor;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Color _baseColor;

        private void Awake()
        {
            if (spriteRenderer)
            {
                _baseColor = spriteRenderer.color;
            }
        }

        public async UniTask Highlight(bool isAttack)
        {
            await spriteRenderer.DOColor(isAttack ? attackColor : moveColor, duration);
            await spriteRenderer.DOColor(_baseColor, duration);
        }
    }
}