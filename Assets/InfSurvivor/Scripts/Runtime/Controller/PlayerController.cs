using System.Collections.Generic;
using System.Linq;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using UnityEngine;

namespace InfSurvivor.Runtime.Controller
{
    [DisallowMultipleComponent]
    public abstract class PlayerController : BaseController, IColliderTrigger
    {
        #region 애니메이션 파라미터 (hash)
        public readonly int ANIM_FLOAT_MOVE_X = Animator.StringToHash("MoveX");
        public readonly int ANIM_FLOAT_MOVE_Y = Animator.StringToHash("MoveY");
        public readonly int ANIM_FLOAT_SPEED = Animator.StringToHash("Speed");
        public readonly int ANIM_BOOL_SWORD_ATTACKING = Animator.StringToHash("SwordAttacking");
        public readonly int ANIM_TRIGGER_SWORD_ATTACK = Animator.StringToHash("SwordAttack");
        #endregion

        #region Components
        public List<Animator> Animators { get; private set; }
        public List<SpriteRenderer> Renderers { get; private set; }
        #endregion

        public Vector2 LastFacingDir { get; protected set; } = Vector2.down;
        // x == -1(L), 1(R)
        // y == 1(U), -1(D)
        private Vector2 direction4;
        public Vector2 Dir4 => direction4;

        #region Unity Events
        protected override void Awake()
        {
            base.Awake();
            Animators = GetComponentsInChildren<Animator>().ToList();
            Renderers = GetComponentsInChildren<SpriteRenderer>().ToList();
        }

        protected override void Start()
        {
            base.Start();
            MoveSpeed = 10f;
        }
        #endregion

        public override void ApplyFacingDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude <= 0.01f)
            {
                return;
            }

            float absX = Mathf.Abs(dir.x);
            float absY = Mathf.Abs(dir.y);

            if (absX >= absY)
            {
                AnimationSetFloat(ANIM_FLOAT_MOVE_X, 1f);
                AnimationSetFloat(ANIM_FLOAT_MOVE_Y, 0f);
                direction4.x = Mathf.Sign(dir.x);
                direction4.y = 0f;
            }
            else
            {
                AnimationSetFloat(ANIM_FLOAT_MOVE_X, 0f);
                AnimationSetFloat(ANIM_FLOAT_MOVE_Y, Mathf.Sign(dir.y));
                direction4.x = 0f;
                direction4.y = Mathf.Sign(dir.y);
            }

            if (dir.x != 0)
            {
                SetFlip(dir.x > 0f);
            }
            else
            {
                SetFlip(false);
            }

            LastFacingDir = dir;
        }

        #region Animation Parameter
        public void AnimationSetTrigger(int id)
        {
            Animators.ForEach(anim => anim.SetTrigger(id));
        }

        public void AnimationSetFloat(int id, float value)
        {
            Animators.ForEach(anim => anim.SetFloat(id, value));
        }

        public void AnimationSetBool(int id, bool value)
        {
            Animators.ForEach(anim => anim.SetBool(id, value));
        }
        #endregion

        #region SpriteRenderer
        public void SetFlip(bool right)
        {
            Renderers.ForEach(sp => sp.flipX = right);
        }
        #endregion

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
        }
#endif
    }

}