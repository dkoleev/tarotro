using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tarotro.Game.View {
    [Serializable]
    public class SpriteAnimation {
        public string name;
        public Sprite[] frames;
        public float frameRate = 12f;
        public bool loop;
    }

    public class SpriteSheetAnimator : MonoBehaviour {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteAnimation[] animations;
        [SerializeField] private string defaultAnimation;

        private SpriteAnimation _current;
        private int _currentFrame;
        private float _timer;
        private bool _finished;

        private void Start() {
            if (!string.IsNullOrEmpty(defaultAnimation)) {
                Play(defaultAnimation);
            }
        }

        private void Update() {
            if (_current == null || _current.frames.Length == 0) return;
            if (_finished) return;

            _timer += Time.deltaTime;
            var frameDuration = 1f / _current.frameRate;

            if (_timer < frameDuration) return;

            _timer -= frameDuration;
            _currentFrame++;

            if (_currentFrame >= _current.frames.Length) {
                if (_current.loop) {
                    _currentFrame = 0;
                } else {
                    _currentFrame = _current.frames.Length - 1;
                    _finished = true;
                    return;
                }
            }

            spriteRenderer.sprite = _current.frames[_currentFrame];
        }

        public void Play(string animationName) {
            var anim = FindAnimation(animationName);
            if (anim == null) return;

            StartAnimation(anim);
        }

        public async UniTask PlayAsync(string animationName, CancellationToken ct = default) {
            var anim = FindAnimation(animationName);
            if (anim == null) return;

            StartAnimation(anim);

            if (anim.loop) return;

            await UniTask.WaitUntil(() => _finished, cancellationToken: ct);
        }

        public void Stop() {
            _current = null;
            _finished = true;
        }

        private SpriteAnimation FindAnimation(string animationName) {
            foreach (var anim in animations) {
                if (anim.name == animationName) return anim;
            }

            return null;
        }

        private void StartAnimation(SpriteAnimation anim) {
            _current = anim;
            _currentFrame = 0;
            _timer = 0f;
            _finished = false;

            if (anim.frames.Length > 0) {
                spriteRenderer.sprite = anim.frames[0];
            }
        }
    }
}
