using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

using Random = UnityEngine.Random;

using CookingPrototype.Kitchen;

namespace CookingPrototype.Controllers {
	public class CustomersController : MonoBehaviour {

		public static CustomersController Instance { get; private set; }

		public int                 CustomersTargetNumber = 15;
		public float               CustomerWaitTime      = 18f;
		public float               CustomerSpawnTime     = 3f;
		public List<CustomerPlace> CustomerPlaces        = null;

		[HideInInspector]
		public int TotalCustomersGenerated { get; private set; } = 0;

		public event Action TotalCustomersGeneratedChanged;

		const string CUSTOMER_PREFABS_PATH = "Prefabs/Customer";

		float _timer = 0f;
		Stack<List<Order>> _orderSets;

		bool HasFreePlaces {
			get { return CustomerPlaces.Any(x => x.IsFree); }
		}

		public bool IsComplete {
			get {
				return TotalCustomersGenerated >= CustomersTargetNumber && CustomerPlaces.All(x => x.IsFree);
			}
		}

		void Awake() {
			if ( Instance != null ) {
				Debug.LogError("Another instance of CustomersController already exists!");
			}
			Instance = this;
		}

		void OnDestroy() {
			if ( Instance == this ) {
				Instance = null;
			}
		}

		void Start() {
			Init();
		}

		void Update() {
			if ( !HasFreePlaces ) {
				return;
			}

			_timer += Time.deltaTime;

			if ( (TotalCustomersGenerated >= CustomersTargetNumber) || (!(_timer > CustomerSpawnTime)) ) {
				return;
			}

			SpawnCustomer();
			_timer = 0f;
		}

		void SpawnCustomer() {
			var freePlaces = CustomerPlaces.FindAll(x => x.IsFree);
			if ( freePlaces.Count <= 0 ) {
				return;
			}

			var place = freePlaces[Random.Range(0, freePlaces.Count)];
			place.PlaceCustomer(GenerateCustomer());
			TotalCustomersGenerated++;
			TotalCustomersGeneratedChanged?.Invoke();
		}

		Customer GenerateCustomer() {
			var customerGo = Instantiate(Resources.Load<GameObject>(CUSTOMER_PREFABS_PATH));
			var customer   = customerGo.GetComponent<Customer>();

			var orders = _orderSets.Pop();
			customer.Init(orders);

			return customer;
		}

		Order GenerateRandomOrder() {
			var oc = OrdersController.Instance;
			return oc.Orders[Random.Range(0, oc.Orders.Count)];
		}

		public void Init() {
			var totalOrders = 0;
			_orderSets = new Stack<List<Order>>();
			for ( var i = 0; i < CustomersTargetNumber; i++ ) {
				var orders = new List<Order>();
				var ordersNum = Random.Range(1, 4);
				for ( var j = 0; j < ordersNum; j++ ) {
					orders.Add(GenerateRandomOrder());
				}
				_orderSets.Push(orders);
				totalOrders += ordersNum;
			}
			CustomerPlaces.ForEach(x => x.Free());
			_timer = 0f;

			TotalCustomersGenerated = 0;
			TotalCustomersGeneratedChanged?.Invoke();
			 
			GameplayController.Instance.OrdersTarget = totalOrders - 2;
		}

		/// <summary>
		/// ?????????????????? ???????????????????? ????????????????????
		/// </summary>
		/// <param name="customer"></param>
		public void FreeCustomer(Customer customer) {
			var place = CustomerPlaces.Find(x => x.CurCustomer == customer);
			if ( place == null ) {
				return;
			}
			place.Free();
			GameplayController.Instance.CheckGameFinish();
		}


		/// <summary>
		///  ???????????????? ?????????????????? ???????????????????? ?? ???????????????? ?????????????? ?? ???????????????????? ???????????????????? ???????????????? ????????????????.
		///  ???????? ?? ???????????????????? ?????? ?????????????????? ???????????????????? ?????????? ???? ????????????, ???? ?????????????????? ??????.
		/// </summary>
		/// <param name="order">??????????, ?????????????? ???????????????? ????????????</param>
		/// <returns>???????? - ??????????????????, ?????????????? ???? ?????????????? ???????????? ??????????</returns>
		public bool ServeOrder(Order order) {

			List<CustomerOrderPlace> customerOrderPlacesList = new List<CustomerOrderPlace>();
			customerOrderPlacesList.AddRange(FindObjectsOfType<CustomerOrderPlace>());
			var neededOrders = customerOrderPlacesList.FindAll(x => x.CurOrder == order);

			List<Customer> customersList = new List<Customer>();
			customersList.AddRange(FindObjectsOfType<Customer>());
			List<Customer> customersWithNeededOrderList = new List<Customer>();

			if ( neededOrders.Count == 0 )
				return false;
			else {
				for(int i = 0; i < customersList.Count; i++ ) {
					for(int j = 0; j < customersList[i].OrderPlaces.Count; j++) {
						if(neededOrders.IndexOf(customersList[i].OrderPlaces[j]) >= 0) {
							if(customersWithNeededOrderList.IndexOf(customersList[i]) <0) {
								customersWithNeededOrderList.Add(customersList[i]);
							}
						}
					}
				}
			}

			List<float> waitWaitTimeList = new List<float>();
			for(int i = 0; i < customersWithNeededOrderList.Count; i++ ) {
				waitWaitTimeList.Add(customersWithNeededOrderList[i].WaitTime);
			}

			var index = waitWaitTimeList.IndexOf(waitWaitTimeList.Min());

			customersWithNeededOrderList[index].ServeOrder(order);

			if ( customersWithNeededOrderList[index].IsComplete )
				FreeCustomer(customersWithNeededOrderList[index]);

			/*
			for(int i =0; i < neededOrders.Count; i++) {
				Debug.Log(neededOrders[i].name);
			}
			*/



			return true;

			//throw  new NotImplementedException("ServeOrder: this feature is not implemented.");
		}
	}
}
