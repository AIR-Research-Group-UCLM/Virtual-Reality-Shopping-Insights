using UnityEngine;

public class RellocateUponCollision : MonoBehaviour
{
    [SerializeField] private GameObject _shoppingCart;
    [SerializeField] private GameObject _bin;
    public Vector3 genericPositionShoppingCart;
    public Vector3 genericPositionBin;
    public Quaternion genericRotationShoppingCart;
    public Quaternion genericRotationBin;

    public void RellocateObjects()
    {
        _shoppingCart.transform.position = genericPositionShoppingCart;
        _shoppingCart.transform.rotation = genericRotationShoppingCart;
        if (_bin != null)
        {
            _bin.transform.position = genericPositionBin;
            _bin.transform.rotation = genericRotationBin; 
        }
    }
}
