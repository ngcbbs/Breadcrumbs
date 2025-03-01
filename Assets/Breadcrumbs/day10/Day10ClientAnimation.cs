using Unity.Netcode.Components;
using UnityEngine;

namespace Day10 {
    public class Day10ClientAnimation : NetworkAnimator {
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}