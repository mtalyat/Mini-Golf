using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MiniGolf
{
    internal abstract class GameObject : DrawableGameComponent
    {
        private Vector2 _localPosition = Vector2.Zero;
        public Vector2 LocalPosition
        {
            get => _localPosition;
            set => SetPosition(value);
        }

        private Vector2 _localScale = Vector2.One;
        public Vector2 LocalScale
        {
            get => _localScale;
            set => SetScale(value);
        }

        private Vector2 _localSize = Vector2.One;
        public Vector2 LocalSize
        {
            get => _localSize;
            set => SetSize(value);
        }

        public Vector2 LocalCenter
        {
            get => GetCenter();
            set => SetCenter(value);
        }

        private float _localRotation = 0.0f;
        public float LocalRotation
        {
            get => _localRotation;
            set => SetRotation(value);
        }

        private GameObject _parent;
        public GameObject Parent
        {
            get => _parent;
            set => SetParent(value);
        }

        private readonly List<GameObject> _children;
        public List<GameObject> Children => _children;

        private readonly HashSet<IGameComponent> _components;

        public GameObject(Game game) : base(game)
        {
            // init the rest
            _parent = null;
            _children = new List<GameObject>();
            _components = new HashSet<IGameComponent>();
        }

        public virtual void Destroy()
        {
            // set null parent
            SetParent(null);

            // destroy all children first
            DestroyChildren();

            // remove self from game
            UnloadContent();
        }

        protected override void OnVisibleChanged(object sender, EventArgs args)
        {
            // set all children to the same visibility
            foreach(GameObject child in Children)
            {
                child.Visible = Visible;
            }

            base.OnVisibleChanged(sender, args);
        }

        public void DestroyChildren()
        {
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                _children[i].Destroy();
            }
            _children.Clear();
        }

        public override void Update(GameTime gameTime)
        {
            // update all children
            for(int i = _children.Count - 1; i >= 0; i--)
            {
                _children[i].Update(gameTime);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // draw children
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                _children[i].Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        protected virtual void SetPosition(Vector2 position)
        {
            _localPosition = position;
        }

        protected virtual void SetCenter(Vector2 center)
        {
            SetPosition(center - (_localSize * _localScale * 0.5f).Rotate(MathHelper.ToRadians(LocalRotation)));
        }

        protected virtual void SetScale(Vector2 scale)
        {
            _localScale = scale;
        }

        protected virtual void SetSize(Vector2 size)
        {
            _localSize = size;
        }

        protected virtual void SetRotation(float rotation)
        {
            _localRotation = rotation;
        }

        public void SetOrientation(Vector2 position, Vector2 scale, Vector2 size, float rotation)
        {
            SetPosition(position);
            SetScale(scale);
            SetSize(size);
            SetRotation(rotation);
        }

        public void SetOrientation(GameObject other) => SetOrientation(other.LocalPosition, other.LocalScale, other.LocalSize, other.LocalRotation);

        public void SetParent(GameObject parent)
        {
            // cannot set parent to self
            if(parent == this)
            {
                _parent = null;
                return;
            }

            // if has a parent, remove from that parent
            if(_parent != null)
            {
                _parent.Children.Remove(this);
            }

            _parent = parent;

            // if there is a new parent, add to that parent's children
            if(_parent != null)
            {
                _parent.Children.Add(this);
            }
        }

        public Vector2 GetGlobalPosition()
        {
            return ((_parent?.GetGlobalPosition() ?? Vector2.Zero) + LocalPosition * (_parent?.GetGlobalScale() ?? Vector2.One));
        }

        public Vector2 GetGlobalScale()
        {
            return (_parent?.GetGlobalScale() ?? Vector2.One) * LocalScale;
        }

        public Vector2 GetGlobalSize()
        {
            return LocalSize * GetGlobalScale();
        }

        public float GetGlobalRotation()
        {
            return (_parent?.GetGlobalRotation() ?? 0.0f) + LocalRotation;
        }

        public virtual Vector2 GetGlobalCenter()
        {
            return GetGlobalPosition() + (GetGlobalSize() * 0.5f).Rotate(MathHelper.ToRadians(GetGlobalRotation()));
        }

        protected virtual Vector2 GetCenter()
        {
            return _localPosition + (_localSize * _localScale * 0.5f).Rotate(MathHelper.ToRadians(_localRotation));
        }

        public virtual Hitbox GetHitbox()
        {
            return new Hitbox(GetGlobalPosition(), GetGlobalSize(), GetGlobalRotation());
        }

        public bool IsSquare()
        {
            Vector2 size = GetGlobalSize();

            return Math.Abs(size.X) == Math.Abs(size.Y);
        }
    }
}
