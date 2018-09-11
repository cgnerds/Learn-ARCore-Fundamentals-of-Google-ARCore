using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

[RequireComponent(typeof(AudioSource))]
public class EnvironmentalScanner : MonoBehaviour
{
    public NeuralNet net;
    public List<DataSet> dataSets;
    private float min = float.MaxValue;
    private float maxRange = float.MinValue;
    private float[] inputs;
    private double[] output;
    private double temp;
    private bool warning;
    private AudioSource audioSource;
    private double lastTimestamp;

    public void Awake()
    {
        int numInputs, numHiddenLayers, numOutputs;
        numInputs = 1; numHiddenLayers = 4; numOutputs = 1;
        net = new NeuralNet(numInputs, numHiddenLayers, numOutputs);
        dataSets = new List<DataSet>();
    }

    // Use this for initialization
    void Start()
    {
        dataSets.Add(new DataSet(new double[] { 1.0, 0.1, 0.0 }, new double[] { 0.0, 1.0, 1.0 }));
        net.Train(dataSets, 0.001);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (warning)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }

        min = float.MaxValue;
        if (Frame.PointCloud.IsUpdatedThisFrame)
        {
            if (Frame.PointCloud.PointCount > 0)
            {
                // find min 
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    Vector3 minus = Frame.PointCloud.GetPoint(i);
                    var rng = Mathf.Clamp01((minus - transform.parent.parent.transform.position).magnitude);
                    min = Mathf.Min(rng, min);
                }

                // compute output
                output = net.Compute(new double[] { (double)min });
                if (output.Length > 0)
                {
                    warning = output[0] > 0.001;
                }
                else
                {
                    warning = false;
                }
            }
        }
    }
}
