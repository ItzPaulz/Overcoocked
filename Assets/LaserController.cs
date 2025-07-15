using UnityEngine;

public class LaserController : MonoBehaviour
{
    private Transform _target;
    private float _speed;
    private GameObject _impactFX;

    public void Init(Transform target, float speed, GameObject impactFX)
    {
        _target = target;
        _speed = speed;
        _impactFX = impactFX;
        if (_target) transform.LookAt(_target);
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (!_target) { Destroy(gameObject); return; }
        transform.position += transform.forward * _speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (other.TryGetComponent(out PlayerController pc)) pc.TakeDamage(1);
        if (_impactFX) Destroy(Instantiate(_impactFX, transform.position, Quaternion.identity), 3f);
        Destroy(gameObject);
    }
}
