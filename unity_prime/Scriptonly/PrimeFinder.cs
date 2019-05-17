using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Collections.LowLevel.Unsafe;

/*
 Simple demonstration of Unity's Job system by Gergely Tenk (github.com/tenkiX)
 
 Objective: summarize how many prime numbers is there in a randomly generated sample
 
 using Unity 2019.1
 */

public class PrimeFinder : MonoBehaviour
{
    [Header("Finding primes in a generated sample of this size:")]
    [SerializeField]
    int inputSize = 100;
    [Header("Make sure your inputsize can be divided with number of threads")]
    [SerializeField]
    int numberOfThreads = 4;
    [SerializeField]
    Text results;
    ParallelJob jobData;
    JobHandle handle;
  
    public void FindPrimesInAGeneratedSample()
    {
        results.text = ("\n Starting...");
        //start a stopper to benchmark
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        NativeArray<int> input = new NativeArray<int>(inputSize, Allocator.TempJob);
        //store data how many prime numbers we found -> make an array element for each thread to avoid racing (multiple threads trying to write the same element can cause trouble)
        NativeArray<int> numberOfFoundPrimes = new NativeArray<int>(numberOfThreads, Allocator.TempJob);
        
        //initialize the sample
        for (int i = 0; i < inputSize; i++)
        {
            input[i] = UnityEngine.Random.Range(0, 10);
        }
        jobData = new ParallelJob();
        jobData.input = input;
        jobData.nthreads = numberOfThreads;
        jobData.inputSize = inputSize;
        jobData.foundPrimes = numberOfFoundPrimes;

        //pass to scheduler - divide the input interval with the threads so we get (thread number) of chunks and we can calculate thread id's in the Execute method
        //(roughly the same as static scheduling)
        handle = jobData.Schedule(inputSize,  inputSize / numberOfThreads);

        //waiting for all threads to finish
        handle.Complete();

        //summarize the total prime numbers we found on each thread
        int totalFoundPrimes = 0;
        for (int i = 0; i < numberOfThreads; i++)
        {
             totalFoundPrimes += jobData.foundPrimes[i];
        }

        //stop our stopper
        sw.Stop();
        results.text+=("\n Done! Elapsed time: " + sw.ElapsedMilliseconds / 1000f + "\n Found: " + totalFoundPrimes);

        // disposal of nativearrays
        input.Dispose();
        numberOfFoundPrimes.Dispose();
    }
}


public struct ParallelJob : IJobParallelFor
{
    [ReadOnly]
    public int inputSize;
    [ReadOnly]
    public int nthreads;
    [ReadOnly]
    public NativeArray<int> input;

    //we must disable automatic behaviour check if we are using our own self-managed thread indexing
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<int> foundPrimes;

    public void Execute(int elementNumber)
    {
        //testing if number is prime or not
        int ceilOfNumber;
        ceilOfNumber = Mathf.CeilToInt(Mathf.Sqrt(input[elementNumber]));
        for (int j = 2; j <= ceilOfNumber; j++)
        {
            if ((input[elementNumber] % j) == 0) break; //it is not a prime number, so we can break the loop
            if (ceilOfNumber == j)
            {
                //increment our result element of [calculated thread Id] because we found a prime number
                foundPrimes[(elementNumber / (inputSize / nthreads))] ++;

                //if we are benchmarking times, logging shouldn't be used
                //Debug.Log("i am thread:" + (elementNumber / (inputSize / nthreads)) + " and found a prime: "+ input[elementNumber]);
            }
        }
    }
}


