using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;


public class DeliveryOrderSystem : MonoBehaviour
{
    [Header("주문 설정")]
    public float ordergenrateInterval = 15f;
    public int maxActiveOrders = 8;

    [Header("게임 상태")]
    public int totalOrdersGenerated = 0;
    public int completedOrders = 0;
    public int expiredOrders = 0;


    // 주문 리스트
    private List<DeliveryOrder> currentOrders = new List<DeliveryOrder>();

    // Building 참조
    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();


    [System.Serializable]
    // Event 시스템
    public class OrderSystemEvents
    {
        public UnityEvent<DeliveryOrder> OnNewOrderAdded;
        public UnityEvent<DeliveryOrder> OnOrderPickUp;
        public UnityEvent<DeliveryOrder> OnOrderCompleted;
        public UnityEvent<DeliveryOrder> OnOrderExpired;
    }

    public OrderSystemEvents orderEvents;
    public DeliveryDriver driver;


    void Start()
    {
         driver = FindFirstObjectByType<DeliveryDriver>()
;        FindAllBuilding();

        // 초기 주문 생성
        StartCoroutine(GenerateInitialOrders());

        // 주기적 주문 생성
        StartCoroutine(orderGenerator());

        // 완료 체크
        StartCoroutine(ExipiredOrderChecker());

    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20 , 10, 400, 1300));

        GUILayout.Label("=== 배달 주문 ===");
        GUILayout.Label($"활성 주문 : {currentOrders.Count} 개 ");
        GUILayout.Label($"픽업 대기 : {GetPickWatingCount()} 개 ");
        GUILayout.Label($"배달 대기 : {GetDeliveryWatingCount()} 개 ");
        GUILayout.Label($"완료 : {completedOrders} 개 | 만료 : {expiredOrders}");

        GUILayout.Space(30);
        
        foreach(DeliveryOrder order in currentOrders)
        {
            string status = order.state == OrderState.WaitingPickup ? "팩업 대기" : "배달대기";
            float timeLeft = order.GetRemainingTime();

            GUILayout.Label($"#{order.orderld} : {order.restaurantName} -> {order.customerName}");
            GUILayout.Label($"{status} | {timeLeft:F0} 초 남음");
        }

        GUILayout.EndArea();
    }



    void FindAllBuilding()
    {
        Building[] allBuildings = FindObjectsByType<Building>(FindObjectsSortMode.None);
            
        foreach(Building building in allBuildings)
        {
            if(building.BuildingType == BuildingType.Restaurant)
            {
                restaurants.Add(building);
            }
            else if(building.BuildingType == BuildingType.Customer)
            {
                customers.Add(building);
            }
        }

        Debug.Log($"음식점 {restaurants.Count} 개 , 고객 {customers.Count} 명 발견");
    }


    void CreateNewOrder()
    {
        if (restaurants.Count == 0 || customers.Count == 0) return;

        // 랜덤 음식점과 고객 선택
        Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
        Building randomCustomer = customers[Random.Range(0, customers.Count)];

        float reward = Random.Range(3000f, 8000f);

        DeliveryOrder newOreder = new DeliveryOrder(++totalOrdersGenerated, randomRestaurant, randomCustomer, reward);

        currentOrders.Add(newOreder);
        orderEvents.OnNewOrderAdded?.Invoke(newOreder);
    }

    
    void PickupOrder(DeliveryOrder order)
    {
        order.state = OrderState.PickedUp;
        orderEvents.OnOrderPickUp?.Invoke(order);
    }

    void CompleteOrder(DeliveryOrder order)
    {
        order.state = OrderState.Completed;
        completedOrders++;

        // 보상 지급
        if(driver != null)
        {
            driver.AddMoney(order.reward);
        }

        // 완료된 주문 제거
        currentOrders.Remove(order);
        orderEvents.OnOrderCompleted?.Invoke(order);
    }


    void ExpireOrder(DeliveryOrder order)
    {
        order.state = OrderState.Eprired;
        expiredOrders++;

        currentOrders.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }


    // UI 정보 제공
    public List<DeliveryOrder> GetCurrentOrders()
    {
        return new List<DeliveryOrder>(currentOrders);
    }

    public int GetPickWatingCount()
    {
        int count = 0;
        foreach(DeliveryOrder order in currentOrders)
        {
            if (order.state == OrderState.WaitingPickup) count++;
        }
        return count;
    }

    public int GetDeliveryWatingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.state == OrderState.PickedUp) count++;
        }
        return count;
    }

    DeliveryOrder FindOrderForPickUp(Building restaurant)
    {
        foreach(DeliveryOrder order in currentOrders)
        {
            if(order.restaurantBuilding == restaurant && order.state == OrderState.WaitingPickup)
            {
                return order;
            }
        }
        return null;
    }


    DeliveryOrder FindOrderForDelivery(Building customer)
    {
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.restaurantBuilding == customer && order.state == OrderState.PickedUp)
            {
                return order;
            }
        }
        return null;
    }

    public void OnDriverEnteredRestaurant(Building restaurant)
    {
        DeliveryOrder orderToPickup = FindOrderForPickUp(restaurant);

        if(orderToPickup != null)
        {
            PickupOrder(orderToPickup);
        }
    }


    public void OnDriverEnteredCustorm(Building customer)
    {
        DeliveryOrder orderToDeliver = FindOrderForDelivery(customer);

        if (orderToDeliver != null)
        {
            CompleteOrder(orderToDeliver);
        }
    }


    IEnumerator GenerateInitialOrders()
    {
        yield return new WaitForSeconds(1f);

        for(int i = 0; i < 3; i++)
        {
            CreateNewOrder();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator orderGenerator()
    {
        while(true)
        {
            yield return new WaitForSeconds(ordergenrateInterval);

            if(currentOrders.Count < maxActiveOrders)
            {
                CreateNewOrder();
            }
        }
    }


    IEnumerator ExipiredOrderChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            List<DeliveryOrder> expiredOrders = new List<DeliveryOrder>();

            foreach(DeliveryOrder order in currentOrders)
            {
                if (order.IsExpried() && order.state != OrderState.Completed)
                {
                    expiredOrders.Add(order);
                }
            }

            foreach(DeliveryOrder expired in expiredOrders)
            {
                ExpireOrder(expired);
            }
        }
    }


}
