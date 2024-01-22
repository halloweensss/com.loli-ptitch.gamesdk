using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseEntityElement : MonoBehaviour
{
    [SerializeField] private TMP_Text _idText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Button _buttonBuy;

    private string _id;

    public string ID => _id;
    public event Action<PurchaseEntityElement> OnBuyClick;

    public void Initialize(string id, string name, string price)
    {
        _id = id;
        _idText.text = id;
        _priceText.text = price;
        _nameText.text = name;
    }

    public void SetActiveButton(bool active)
    {
        _buttonBuy.interactable = active;
    }

    private void OnBuyClickHandler()
    {
        OnBuyClick?.Invoke(this);
    }

    private void OnEnable()
    {
        _buttonBuy.onClick.AddListener(OnBuyClickHandler);
    }

    private void OnDisable()
    {
        _buttonBuy.onClick.RemoveListener(OnBuyClickHandler);
    }
}
