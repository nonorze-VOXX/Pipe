using System;
using System.Collections.Generic;
using fft;
using GameSetting;
using Unity.VisualScripting;
using UnityEngine;

namespace Pipe
{
    public enum PipeViewStatus
    {
        Spinning,
        ColdDown,
        Normal
    }

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
        private readonly float spinTime = 0.1f;
        private readonly float triggerColdDown = 5f;
        private List<SpriteRenderer> _childSprites;
        private float _fromAngle;
        private GameManager _gameManager;
        private UnitPipe _originUnitPipe;
        private GameObject _pipeEnd;

        private List<GameObject> _pipeLine;
        // private int rotationTimes;

        private PipeViewStatus _pipeViewStatus = PipeViewStatus.Normal;

        private PuzzleType _puzzleType;

        private float _toAngle;
        private float spinTimer = float.MaxValue;

        private float triggerTimer = float.MaxValue;

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

            var count = transform.childCount;
            for (var i = 1; i < count; i++)
                _childSprites.Add(
                    transform.GetChild(i).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>());

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

            Trigger(Color.white);
            _gameManager.UpdatePipe();
        }

        public void Trigger(Color color)
        {
            triggerTimer = 0;
            spinTimer = 0;
            _fromAngle = transform.rotation.eulerAngles.z;
            _toAngle = transform.rotation.eulerAngles.z + 360.0f / _originUnitPipe.GetNeighbor().Count;
            ;
            SetPipeViewStatus(PipeViewStatus.Spinning, color);
        }

        public void SoundUpdate()
        {
            if (spinTimer <= spinTime)
            {
                spinTimer += Time.deltaTime;
                var timePercent = spinTimer / spinTime;
                var rotation = transform.rotation.eulerAngles;
                rotation.z = Mathf.Lerp(_fromAngle, _toAngle, timePercent);
                transform.rotation = Quaternion.Euler(rotation);
            }
            else
            {
                SetPipeViewStatus(PipeViewStatus.ColdDown);
            }

            if (triggerTimer <= triggerColdDown)
                triggerTimer += Time.deltaTime;
            else
                SetPipeViewStatus(PipeViewStatus.Normal);
        }

        public bool IsTrigger()
        {
            return triggerTimer <= triggerColdDown;
        }

        private void changeBgColor(Color color)
        {
            for (var i = 0; i < _childSprites.Count; i++) _childSprites[i].color = color;
        }

        public void ChangeOneBgColor(Color color)
        {
            _childSprites[0].color = color;
        }

        public void RotateOverClock(bool b)
        {
            _originUnitPipe.RotateOverClock(b);
        }

        #region setter

        private void SetPipeViewStatus(PipeViewStatus pipeViewStatus, Color color)
        {
            switch (pipeViewStatus)
            {
                case PipeViewStatus.Spinning:
                    changeBgColor(color);
                    _pipeViewStatus = PipeViewStatus.Spinning;
                    RotateOverClock(true);
                    break;
                case PipeViewStatus.ColdDown:
                    changeBgColor(color);
                    _pipeViewStatus = PipeViewStatus.ColdDown;
                    break;
                case PipeViewStatus.Normal:
                    changeBgColor(color);
                    _pipeViewStatus = PipeViewStatus.Normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pipeViewStatus), pipeViewStatus, null);
            }
        }

        private void SetPipeViewStatus(PipeViewStatus pipeViewStatus)
        {
            switch (pipeViewStatus)
            {
                case PipeViewStatus.Spinning:
                    SetPipeViewStatus(pipeViewStatus, Color.yellow);
                    break;
                case PipeViewStatus.ColdDown:
                    SetPipeViewStatus(pipeViewStatus, Color.gray);
                    break;
                case PipeViewStatus.Normal:
                    SetPipeViewStatus(pipeViewStatus, Color.white);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pipeViewStatus), pipeViewStatus, null);
            }
        }

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