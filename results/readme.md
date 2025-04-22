# 🚀 Quantum Circuit Cutting with HyPAQ

This repository presents key results from applying the **HyPAQ (Hypergraphic Partitioning for Static and Adaptive Quantum Circuits)** methodology for **quantum circuit cutting**. HyPAQ enables large quantum circuits to be executed on smaller quantum hardware by partitioning the circuit into subcircuits using advanced **graph** and **hypergraph partitioning techniques**. 

The following sections illustrate the performance of HyPAQ in terms of **entanglement reduction** and **partitioning time**.

---

## 🖼️ Figure 1: Reduction Percentage in Ebits vs Circuit Width

<p align="center">
  <img src="https://github.com/hypaq/hypaq/blob/main/images/result2.png" alt="Reduction Percentage vs Circuit Width" width="80%">
</p>

This figure shows the **percentage reduction in entanglement bits (ebits)** as a function of **circuit width (n)**, achieved through the **HyPAQ** cutting strategy. Ebits represent the quantum communication overhead across circuit partitions.

- For **small circuits (low n)**, HyPAQ achieves **high reductions in ebits**, peaking around **40%**.
- As the **circuit width increases**, the reduction percentage steadily decreases, approaching **0-5%** for larger circuits (n > 80).
- This indicates that HyPAQ is particularly effective at **reducing entanglement requirements for small to medium circuits**, which is crucial for resource-constrained quantum devices.

---

## 🖼️ Figure 2: Impact of Different Optimization Levels on Ebit Reduction

<p align="center">
  <img src="https://github.com/hypaq/hypaq/blob/main/images/result3.png" alt="Optimization Levels Comparison" width="80%">
</p>

This figure compares **ebit reduction percentages** for **different compiler optimization levels** applied to the circuits before partitioning:

- **Opt 0**: No optimization.
- **Opt 1-3**: Increasing levels of circuit optimization.

### 🔍 Key insights:

- Across all optimization levels, the **reduction trend remains consistent**.
- While some fluctuations occur at smaller circuit widths, the overall reduction pattern is similar across **Opt 0 to Opt 3**.
- This demonstrates that **HyPAQ's cutting effectiveness is robust**, performing similarly across **varying levels of circuit optimization**.

---

## 🖼️ Figure 3: Partitioning Time Performance (KL vs FM)

<p align="center">
  <img src="https://github.com/hypaq/hypaq/blob/main/images/result1.png" alt="Partitioning Time Comparison (KL vs FM)" width="80%">
</p>

This figure illustrates the **time spent** applying two different partitioning heuristics used within **HyPAQ**:

- **KL (Kernighan-Lin heuristic)**: A classic graph partitioning method.
- **FM (Fiduccia-Mattheyses heuristic)**: A hypergraph partitioning technique.

The four subplots represent different experimental scenarios (e.g., circuit types or parameter variations):

- **KL (graph partitioning)** generally consumes **more time** as the **number of qubits increases**, reflecting the growing complexity of graph partitioning for larger circuits.
- **FM (hypergraph partitioning)** exhibits **lower and more stable execution times**, showing better scalability for partitioning circuits modeled as hypergraphs.
- The **gap between KL and FM times** becomes more noticeable for circuits with **more than 80 qubits**, highlighting the **efficiency advantage of hypergraph partitioning** in larger circuits.

---

## 📝 Conclusion

These results demonstrate the **scalability**, **efficiency**, and **robustness** of the **HyPAQ methodology** for **quantum circuit cutting**, especially emphasizing the benefits of **hypergraph partitioning (FM)** over traditional **graph partitioning (KL)** in handling larger quantum circuits.

---

💡 Feel free to explore the data and methodology further, and contributions or feedback are welcome!
