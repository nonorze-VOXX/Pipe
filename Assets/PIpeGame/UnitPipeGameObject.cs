﻿using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityTools.Vector2;

namespace Pipe
{
    public enum PipeStatus
    {
        Watered,
        Dry,
        Cycling
    }

    public class UnitPipeGameObject : MonoBehaviour
    {
        private List<SpriteRenderer> _childSprites;
        private GameManager _gameManager;
        private UnitPipe _originUnitPipe;
        private GameObject _pipeEnd;
        private List<GameObject> _pipeLine;
        private int _puzzleType;
        private int rotationTimes;

        private void Awake()
        {
            rotationTimes = 0;
            _pipeLine = new List<GameObject>();
            _pipeLine.Add(transform.GetChild(1).GameObject());
            _pipeEnd = transform.GetChild(0).GameObject();
        }

        private void Start()
        {
            var angle = 360 / _puzzleType;
            _pipeEnd.SetActive(_originUnitPipe.GetNumOfConnection() == 1);
            _childSprites = new List<SpriteRenderer>();
            for (var i = 1; i < _puzzleType; i++)
            {
                var line = Instantiate(transform.GetChild(1).GameObject(), transform);
                line.transform.Rotate(new Vector3(0, 0, angle * i));
                _pipeLine.Add(line);
            }

            for (var i = 0; i < transform.childCount; i++)
                _childSprites.Add(transform.GetChild(i).transform.GetChild(0).GetComponent<SpriteRenderer>());

            var index = 1;
            foreach (var up in Vector2List.FourDirection())
                transform.GetChild(index++).GameObject().SetActive(_originUnitPipe.connections[up]);
        }

        private void OnMouseDown()
        {
            if (_gameManager.GetGameFlow() == GameFlow.WIN) return;
            rotationTimes = (rotationTimes + 1) % _puzzleType;
            transform.Rotate(new Vector3(0, 0, 90));
            RotateOverClock(true);
            _gameManager.UpdatePipe();
        }

        public void RotateOverClock(bool b)
        {
            _originUnitPipe.RotateOverClock(b);
        }

        public void SetGameManager(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void SetPuzzleType(int line)
        {
            _puzzleType = line;
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

            foreach (var sprite in _childSprites)
                sprite.color = color;
        }
    }
}