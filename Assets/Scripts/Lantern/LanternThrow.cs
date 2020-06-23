﻿using System.Collections;
using UnityEngine;

namespace Lantern
{
    public class LanternThrow : MonoBehaviour
    {
        [SerializeField] private float _secondsToDespawn = 3;
        [SerializeField] private float _maxDistance = 15f;
        [SerializeField] private Transform _player = null;
        [SerializeField] private GameObject _cursor = null;

        [SerializeField] private Camera one;
        [SerializeField] private Camera left;
        [SerializeField] private Camera right;
        [SerializeField] private AudioClip _lanternThrowSound;
        [SerializeField] private AudioClip _lanternCollisionSound;
        [SerializeField] private AudioSource _lanternThrowSource;
        [SerializeField] private AudioSource _lanternCollisionSource;
        [SerializeField] private AudioSource _abilitySource;

        private GameObject _capturer;
        private Rigidbody _lanternRB;
        private LanternBehaviour behaviour;
        private LineRenderer _line;
        private float _activeTime;

        // Start is called before the first frame update
        private void Start()
        {
            _line = GetComponent<LineRenderer>();
            _lanternRB = GetComponent<Rigidbody>();
            _capturer = GameObject.Find("Lantern_Object");
            _activeTime = 0;
            behaviour = _capturer.GetComponentInChildren<LanternCapture>().lantern;
            _capturer.SetActive(false);
        }

        // Update is called once per frame
        private void Update()
        {
            CheckLanternDespawn();
            LanternThrowing();
            AbilityCast();
        }

        private void CheckLanternDespawn()
        {
            _activeTime = _capturer.activeSelf ?
                _activeTime + Time.deltaTime : 0;

            if ((int)_activeTime % 60 > _secondsToDespawn)
            {
                _capturer.SetActive(false);
            }
        }

        private void LanternThrowing()
        {
            if (!one.isActiveAndEnabled)
            {
                Ray camRay = left.isActiveAndEnabled ?
                    new Ray(_player.position, left.transform.forward) :
                    new Ray(_player.position, right.transform.forward);

                if (Physics.Raycast(camRay, out RaycastHit hit, 100f,
                    LayerMask.GetMask("Level") | LayerMask.GetMask("Default") 
                    | LayerMask.GetMask("Entity")))
                {
                    if (hit.distance < _maxDistance)
                    {
                        _line.enabled = true;
                        _cursor.SetActive(true);

                        float distance = Vector3.Distance
                            (_player.position, hit.point) / 5;

                        Vector3 calcVel = ThrowLantern(hit.point,
                            _player.position, distance / 2);

                        DrawPath(calcVel, distance / 2);

                        _cursor.transform.position = hit.point;

                        if (Input.GetKeyDown(KeyCode.E) && 
                            !behaviour.Colors[1].HasValue)
                        {
                            if (!_capturer.activeSelf)
                            {
                                _capturer.SetActive(true);
                                transform.position = _player.position;

                                _lanternRB.velocity = calcVel;

                                PlaySound(_lanternThrowSource);
                            }   
                        }
                    }
                    else
                    {
                        _line.enabled = false;
                        _cursor.SetActive(false);
                    }
                }
            }
            else
            {
                _line.enabled = false;
                _cursor.SetActive(false);
            }
        }

        private Vector3 ThrowLantern(Vector3 target, Vector3 start, float time)
        {
            Vector3 dis = target - start;
            Vector3 disX = dis;
            disX.y = 0f;

            float sy = dis.y;
            float sx = disX.magnitude;

            float velocityX = sx / time;
            float velocityY = sy / time + 0.5f *
                Mathf.Abs(Physics.gravity.y) * time;

            Vector3 final = disX.normalized;

            final *= velocityX;
            final.y = velocityY;

            return final;
        }

        private void DrawPath(Vector3 sim, float speed)
        {
            _line.positionCount = 1;
            _line.SetPosition(0, _player.position);

            for (int i = 1; i <= 150; i++)
            {
                float simtime = i / (float)30 * speed;

                Vector3 simss = (sim * simtime + Vector3.up * Physics.gravity.y
                    * simtime * simtime / 2f);

                Vector3 point = simss + _player.position;

                _line.positionCount += 1;
                _line.SetPosition(i, point);

                if (Physics.OverlapSphere(point, 0.1f,
                    LayerMask.GetMask("Level") | LayerMask.GetMask("Default") 
                    | LayerMask.GetMask("Entity")).Length > 0 
                    && _line.positionCount > 10f)
                {
                    break;
                }
            }
        }

        private void AbilityCast()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                behaviour.EmptyLantern(false);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                IAbility ability = behaviour.GetAbility();

                if (ability != null)
                {
                    ability.ActivateAbility();
                    ability.PlaySound(_abilitySource);
                    StartCoroutine(CheckAbilityStatus(ability));
                }
            }
        }

        private IEnumerator CheckAbilityStatus(IAbility ability)
        {
            while (!ability.HabilityEnded)
            {
                yield return null;
            }
            behaviour.EmptyLantern(true);
        }

        private void PlaySound(AudioSource _lanternThrowSource)
        {
            _lanternThrowSource.clip = _lanternThrowSound;
            _lanternThrowSource.volume = Random.Range(0.6f, 0.8f);
            _lanternThrowSource.pitch = Random.Range(0.8f, 1f);
            _lanternThrowSource.Play();
        }

        /// <summary>
        /// OnCollisionEnter is called when this collider/rigidbody has begun
        /// touching another rigidbody/collider.
        /// </summary>
        /// <param name="other">The Collision data associated with this collision.</param>
        void OnCollisionEnter(Collision other)
        {
            _lanternCollisionSource.clip = _lanternCollisionSound;
            _lanternCollisionSource.volume = Random.Range(0.3f, 0.5f);
            _lanternCollisionSource.pitch = Random.Range(0.8f, 1f);
            _lanternCollisionSource.Play();
        }
    }
}