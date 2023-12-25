using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace fft
{
    [RequireComponent(typeof(AudioSource))]
    public class FastFourierTransform : MonoBehaviour
    {
        public GameObject cube;
        public FFtConfig fFtConfig;
        private readonly List<GameObject> cubes = new();
        private readonly List<SpriteRenderer> leds = new();
        private readonly float[] samples = new float[512];
        private AudioSource audioSource;

        private SpriteRenderer cubeSprite;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            cubeSprite = cube.GetComponent<SpriteRenderer>();
            for (var i = 0; i < samples.Length; i++)
            {
                var nCube = Instantiate(cube);
                nCube.name = i.ToString();
                nCube.transform.position = new Vector3(i, 0, 0);
                cubes.Add(nCube);
            }

            var x = 0;
            var y = -2;
            foreach (var filter in fFtConfig.filterConfigs)
            {
                var nCube = Instantiate(cube);
                nCube.name = filter.name;
                nCube.transform.position = new Vector3(x, y, 0);
                x += 2;
                leds.Add(nCube.GetComponent<SpriteRenderer>());
            }
        }

        private void Update()
        {
            GetAudio();
            for (var i = 0; i < samples.Length; i++)
            {
                var tmp = cubes[i].transform.position;
                tmp.y = samples[i] * 100;
                cubes[i].transform.position = tmp;
            }

            var index = 0;
            foreach (var filter in fFtConfig.filterConfigs)
            {
                var targetList = samples.Skip(filter.startIndex).Take(filter.endIndex);
                float result;
                switch (filter.type)
                {
                    case FilterType.sum:
                        result = targetList.Sum();
                        break;
                    case FilterType.max:
                        result = targetList.Max();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (result * 1000 > filter.threshold)
                    leds[index].color = Color.red;
                else
                    leds[index].color = Color.white;
                index++;
            }
        }

        private void GetAudio()
        {
            audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        }


        public static int BitReverse(int n, int bits)
        {
            var reversedN = n;
            var count = bits - 1;

            n >>= 1;
            while (n > 0)
            {
                reversedN = (reversedN << 1) | (n & 1);
                count--;
                n >>= 1;
            }

            return (reversedN << count) & ((1 << bits) - 1);
        }

        // public static void FFT(Complex[] buffer)
        // {
        //     var bits = (int)Mathf.Log(buffer.Length, 2);
        //     for (var i = 0; i < buffer.Length / 2; i++)
        //     {
        //         var swapPos = BitReverse(i, bits);
        //         (buffer[i], buffer[swapPos]) = (buffer[swapPos], buffer[i]);
        //     }
        //
        //     for (var N = 2; N < buffer.Length; N <<= 1)
        //     for (var i = 0; i < buffer.Length; i += N)
        //     for (var k = 0; k < N / 2; k++)
        //     {
        //         var evenIndex = i + k
        //             ;
        //         var oddIndex = i + k + N / 2
        //             ;
        //         var even = buffer[evenIndex];
        //         var odd
        //             = buffer[oddIndex];
        //         double term = -2 * Mathf.PI * k / N;
        //         var exp = new Complex(Mathf.Cos((float)term), Mathf.Sin((float)term)) * odd;
        //
        //         buffer[evenIndex] = even + exp;
        //         buffer[oddIndex] = even - exp;
        //     }
        // }
    }
}