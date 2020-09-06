using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.SceneManagement;

public class LorentzAttractor {
    float rho;
    float sigma;
    float beta;
    float x = 1f;
    float y = 1f;
    float z = 1f;
    public LorentzAttractor(float rho = 28,
    float sigma = 10,
     float beta = 8f / 3f) {
        this.rho = rho;
        this.sigma = sigma;
        this.beta = beta;
    }
    public Vector3 next(float delta) {
        float dx = sigma * (y - x);
        float dy = x * (rho - z) - y;
        float dz = x * y - beta * z;

        x += dx * delta;
        y += dy * delta;
        z += dz * delta;
        return new Vector3(x, y, z);
    }
}