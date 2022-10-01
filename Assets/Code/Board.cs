using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private BoxCollider boardObject;
        [SerializeField] private Transform cameraPos;
        [SerializeField] private GameObject pawnsObject;
        [SerializeField] private GameObject aiObject;

        [SerializeField] private PawnGO whitePawn;
        [SerializeField] private PawnGO blackPawn;
        [SerializeField] private GameObject whiteSquare;
        [SerializeField] private GameObject blackSquare;
    }
}