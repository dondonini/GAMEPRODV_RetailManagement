using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckDriver : MonoBehaviour
{
    [SerializeField] List<TruckCrate> crates = new List<TruckCrate>();
    [SerializeField] List<TruckProduct> products = new List<TruckProduct>();
    public float pushForce = 100.0f;
    [Range(0.0f, 5.0f)]
    public float pushVariationAmount = 2.0f;
    public float spawnFrequency = 1.0f;
    [Range(0.0f, 0.1f)]
    public float directionVariation = 0.1f;
    [SerializeField] Transform spawner = null;
    [SerializeField] Animator animator = null;

    [SerializeField] GameObject cratePrefab;

    public bool isSpawning = false;

    MapManager mapManager = null;
    Coroutine activeSpawning = null;

    private void Start()
    {
        mapManager = MapManager.GetInstance();
    }

    public void AddCrate(StockTypes stockType, int stockAmount)
    {
        crates.Add(new TruckCrate(stockType, stockAmount));
    }

    public void AddProduct(StockTypes stockType, int stockAmount)
    {
        products.Add(new TruckProduct(stockType, stockAmount));
    }

    public void AddProduct(StockTypes stockTypes)
    {
        AddProduct(stockTypes, 1);
    }

    public void SpawnCrates()
    {
        if (activeSpawning != null)
            return;

        activeSpawning = StartCoroutine(ISpawnCrates());
    }

    private IEnumerator ISpawnCrates()
    {
        isSpawning = true;

        //GameObject[] tempCrates = crates.ToArray();

        //for (int c = 0; c < tempCrates.Length; c++)
        //{
        //    GameObject newCrate = Instantiate(tempCrates[c]) as GameObject;

        //    newCrate.transform.position = spawner.position;

        //    Rigidbody rb = newCrate.GetComponent<Rigidbody>();
        //    rb.AddForce(-spawner.forward * pushForce);

        //    crates.Remove(tempCrates[c]);

        //    yield return new WaitForSeconds(spawnFrequency);
        //}

        // Unload products
        Debug.Log("Unloading products...", gameObject);
        foreach (TruckProduct product in products)
        {
            int amount = product.stockAmount;

            for (int pCount = 0; pCount < amount; pCount++)
            {
                GameObject newProduct = Instantiate(mapManager.GetStockTypePrefab(product.stockType)) as GameObject;

                newProduct.transform.position = spawner.position;

                Rigidbody rb = newProduct.GetComponent<Rigidbody>();
                rb.AddForce(CalculatePushDirection() * CalculatePushForce());

                yield return new WaitForSeconds(spawnFrequency);
            }
        }

        // Unload crates
        Debug.Log("Unloading crate...", gameObject);
        foreach (TruckCrate crate in crates)
        {
            GameObject newCrate = Instantiate(cratePrefab) as GameObject;

            newCrate.transform.position = spawner.position;

            StockCrate stockCrate = newCrate.GetComponent<StockCrate>();

            stockCrate.SetStockType(crate.stockType);
            int remaining = stockCrate.AddQuantity(crate.stockAmount);

            //if (remaining > 0)
            //{
            //    crates.Add(new TruckCrate(crate.stockType, crate.stockAmount));
            //}

            Rigidbody rb = newCrate.GetComponent<Rigidbody>();
            rb.AddForce(CalculatePushDirection() * CalculatePushForce());

            yield return new WaitForSeconds(spawnFrequency);
        }

        isSpawning = false;

        ClearTruck();

        animator.SetTrigger("Exit");

        activeSpawning = null;

        yield return null;
    }


    public void ClearTruck()
    {
        // Clear truck
        crates.Clear();
        products.Clear();
    }

    float CalculatePushForce()
    {
        return pushForce + Random.Range(-pushVariationAmount, pushVariationAmount);
    }

    Vector3 CalculatePushDirection()
    {
        // Add variation to throwing angle
        Vector3 direction = -Vector3.forward;
        direction.x = Random.Range(-directionVariation, directionVariation);
        direction.y = Random.Range(-directionVariation, directionVariation);

        Debug.DrawRay(spawner.position, direction, Color.red, 1.0f);

        return spawner.TransformVector(direction);
    }

    public Animator GetAnimator()
    {
        return animator;
    }
}
