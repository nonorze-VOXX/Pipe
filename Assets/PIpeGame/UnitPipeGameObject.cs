using System;
using System.Collections.Generic;
using fft;
using GameSetting;
using Unity.VisualScripting;
using UnityEngine;

namespace Pipe
{
    public enum PipeStatus
    {
        Watered,
        Dry,
        Cycling
    }

    public enum PipeChild
    {
        Pipe = 0,
        Background = 1
    }

    public class UnitPipeGameObject : MonoBehaviour, ISoundTriggerable
    {
        private readonly float triggerColdDown = 0.3f;
        private List<SpriteRenderer> _childSprites;
        private GameManager _gameManager;
        private UnitPipe _originUnitPipe;
        private GameObject _pipeEnd;
        private List<GameObject> _pipeLine;

        private PuzzleType _puzzleType;

        private float triggerTimer = float.MaxValue;
        // private int rotationTimes;

        private void Awake()
        {
            // rotationTimes = 0;
            _pipeLine = new List<GameObject>();
            _pipeLine.Add(transform.GetChild(1).GameObject());
            _pipeEnd = transform.GetChild(0).GameObject();
        }

        private void Start()
        {
            var angle = 360 / (int)_puzzleType;
            _pipeEnd.SetActive(_originUnitPipe.GetNumOfConnection() == 1);
            _childSprites = new List<SpriteRenderer>();
            for (var i = 1; i < (int)_puzzleType; i++)
            {
                var line = Instantiate(transform.GetChild(1).GameObject(), transform);
                line.transform.Rotate(new Vector3(0, 0, angle * i));
                _pipeLine.Add(line);
            }

            for (var i = 0; i < transform.childCount; i++)
                _childSprites.Add(transform.GetChild(i).transform.GetChild(0).GetComponent<SpriteRenderer>());

            var index = 1;
            foreach (var up in _originUnitPipe.GetNeighbor())
            {
                transform.GetChild(index).GetChild((int)PipeChild.Pipe).GameObject()
                    .SetActive(_originUnitPipe.connections[up]);
                if (_originUnitPipe.GetNeighbor().Count == 6)
                {
                    var scale = transform.GetChild(index).GetChild((int)PipeChild.Background).transform.localScale;
                    transform.GetChild(index).GetChild((int)PipeChild.Background).transform.localScale = new Vector3(
                        scale.x,
                        scale.x,
                        scale.z
                    );
                }

                index++;
            }
        }

        private void Update()
        {
            SoundUpdate();
        }

        private void OnMouseDown()
        {
            if (_gameManager.GetGameFlow() == GameFlow.WIN) return;
            if (IsTrigger()) return;
            // rotationTimes = (rotationTimes + 1) % (int)_puzzleType;

            Trigger();
            _gameManager.UpdatePipe();
        }

        public void Trigger()
        {
            triggerTimer = 0;
            RotateOverClock(true);
        }

        public void SoundUpdate()
        {
            if (triggerTimer <= triggerColdDown)
            {
                triggerTimer += Time.deltaTime;

                var rotateAngle = 360.0f / _originUnitPipe.GetNeighbor().Count;
                var timePercent = Time.deltaTime / triggerColdDown;
                transform.Rotate(new Vector3(0, 0, rotateAngle * timePercent));
            }
        }

        public bool IsTrigger()
        {
            return triggerTimer <= triggerColdDown;
        }


        public void RotateOverClock(bool b)
        {
            _originUnitPipe.RotateOverClock(b);
        }

        #region setter

        public void SetGameManager(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void SetPuzzleType(PuzzleType puzzleType)
        {
            _puzzleType = puzzleType;
        }

        public void SetUnitPipe(UnitPipe up)
        {
            _originUnitPipe = up;
        }

        public void SetConnectWaterSource(PipeStatus s)
        {
            Color color;
            switch (s)
            {
                case PipeStatus.Watered:
                    color = Color.blue;
                    break;
                case PipeStatus.Dry:
                    color = Color.black;
                    break;
                case PipeStatus.Cycling:
                    color = Color.red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }

            if (_childSprites == null) return;
            foreach (var sprite in _childSprites)
                sprite.color = color;
        }

        #endregion
    }
}