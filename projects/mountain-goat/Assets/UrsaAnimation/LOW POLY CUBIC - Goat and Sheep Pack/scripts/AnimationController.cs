using UnityEngine;

namespace Ursaanimation.CubicFarmAnimals
{
    [RequireComponent(typeof(Animator))]
    public class AnimationController : MonoBehaviour
    {
        public Animator animator;
        public string walkForwardAnimation = "walk_forward";
        public string walkBackwardAnimation = "walk_backwards";
        public string runForwardAnimation = "run_forward";
        public string turn90LAnimation = "turn_90_L";
        public string turn90RAnimation = "turn_90_R";
        public string trotAnimation = "trot_forward";
        public string sittostandAnimation = "sit_to_stand";
        public string standtositAnimation = "stand_to_sit";

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }

        public void SetAnimator(Animator value)
        {
            animator = value != null ? value : GetComponent<Animator>();
        }

        public void PlayIdle()
        {
            PlayState("idle");
        }

        public void PlayWalkForward()
        {
            PlayState(walkForwardAnimation);
        }

        public void PlayWalkBackward()
        {
            PlayState(walkBackwardAnimation);
        }

        public void PlayRunForward()
        {
            PlayState(runForwardAnimation);
        }

        public void PlayTurnLeft()
        {
            PlayState(turn90LAnimation);
        }

        public void PlayTurnRight()
        {
            PlayState(turn90RAnimation);
        }

        public void PlayTrot()
        {
            PlayState(trotAnimation);
        }

        public void PlaySitToStand()
        {
            PlayState(sittostandAnimation);
        }

        public void PlayStandToSit()
        {
            PlayState(standtositAnimation);
        }

        public void PlayState(string stateName)
        {
            if (animator == null || string.IsNullOrEmpty(stateName))
            {
                return;
            }

            animator.Play(stateName, 0, 0f);
        }
    }
}
