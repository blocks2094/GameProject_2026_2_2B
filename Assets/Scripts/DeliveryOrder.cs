using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


// 간단 한 배달 주문
[System.Serializable]
public class DeliveryOrder
{
    public int orderld;
    public string restaurantName;
    public string customerName;


    public Building restaurantBuilding;
    public Building customerBuilding;


    public float orderTime;
    public float timeLimit;
    public float reward;
    public OrderState state;


    // 생성자
   public DeliveryOrder(int id, Building restauranat, Building customer, float rewardAmount)
    {
        orderld = id;
        restaurantBuilding = restauranat;
        customerBuilding = customer;
        restaurantName = restauranat.buildingName;
        customerName = customer.buildingName;
        orderTime = Time.time;
        timeLimit = Random.Range(60f, 120f);
        reward = rewardAmount;
        state = OrderState.WaitingPickup;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, timeLimit - (Time.time - orderTime));
    }

    public bool IsExpried()
    {
        return GetRemainingTime() <= 0f;
    }
}
