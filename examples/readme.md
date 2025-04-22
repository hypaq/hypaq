<div align="center">
  <p align="center">
    <img src="https://github.com/hypaq/hypaq/blob/main/images/hypaq.png" alt="hypaq" width="10%">
  </p>
  <strong>HyPAQ</strong>
</div>

# HyPAQ - Hypergraph Partitioning for Static and Adaptive Quantum Circuits
[![PyPI downloads](https://img.shields.io/pypi/dm/your-package-name?label=PyPI%20downloads)](https://pypi.org/project/hypaq/)
[![Stack Overflow](https://img.shields.io/badge/stackoverflow-Ask%20questions-blue)](https://stackoverflow.com/questions/tagged/hypaq)
[![DOI](https://img.shields.io/badge/DOI-10.1109%2FQCE57702.2023.00055-blue)](https://doi.org/10.48550/arXiv.2504.09318)
[![Contribute](https://img.shields.io/badge/Contribute-Good%20First%20Issue-brightgreen)](https://github.com/hypaq/hypaq/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22)

## 🔍 Examples of Hypergraph-Based Representation for Quantum Circuit Cutting

This section introduces key concepts in **quantum circuit cutting** using **hypergraph representations**, a central methodology in the **HyPAQ framework**. Below, we walk through several illustrative figures demonstrating how quantum circuits are transformed into graph and hypergraph models to enable efficient partitioning across distributed quantum computing systems.

---

### 🖼️ [Figure 1: Graph vs. Hypergraph Representations](https://github.com/hypaq/hypaq/blob/main/images/fig1.png)

<p align="center">
  <img src="https://github.com/hypaq/hypaq/blob/main/images/fig1.png" alt="Graph vs. Hypergraph" width="70%">
</p>

This figure compares **graphs** and **hypergraphs** for modeling quantum circuits:
- **Graph (left)**: Represents **binary relationships** between qubits (e.g., a CX gate between `q2` and `q5`).
- **Hypergraph (middle)**: Captures **multi-qubit gates** (e.g., a CCX gate represented by `e3`), where **hyperedges** connect more than two qubits.
- **Bipartite Graph (right)**: Translates the hypergraph into a bipartite structure, making the relationships between qubits and gates explicit for computational purposes.

Hypergraphs are essential for accurately representing **non-binary interactions** in quantum circuits.

---

### 🖼️ [Figure 2: From Quantum Circuit to Directed Acyclic Graph (DAG)](https://github.com/hypaq/hypaq/blob/main/images/fig2.png)

<p align="center">
  <img src="https://github.com/hypaq/hypaq/blob/main/images/fig2.png" alt="Graph vs. Hypergraph" width="70%">
</p>

This figure shows how a **quantum circuit** (left) can be converted into a **Directed Acyclic Graph (DAG)** (right):
- **Circuit**: Composed of gates and qubits (`q0` to `q5`).
- **DAG**: Represents **dependencies** between operations, with nodes as gates and edges showing the **execution order**. Weights indicate **latency** or **communication costs**.

DAGs help visualize the **flow of computation** and assist in **scheduling** and **partitioning** for distributed execution.

---

### 🖼️ [Figure 3: Binary Gates and Graph-Based Partitioning](https://github.com/hypaq/hypaq/blob/main/images/fig3.png)

<p align="center">
  <img src="https://github.com/hypaq/hypaq/blob/main/images/fig3.png" alt="Graph vs. Hypergraph" width="70%">
</p>

In this example:
- The **circuit (left)** consists of **binary gates** only.
- The **graph (right)** models these pairwise interactions between qubits.

For **binary-only circuits**, graph-based partitioning suffices. However, for **multi-qubit gates**, a **hypergraph** becomes necessary.

---

### 🖼️ [Figure 4: Multi-Qubit Gates and Hypergraph-Based Partitioning](https://github.com/hypaq/hypaq/blob/main/images/fig4.png)

<p align="center">
  <img src="https://github.com/hypaq/hypaq/blob/main/images/fig4.png" alt="Graph vs. Hypergraph" width="70%">
</p>

Here:
- The **quantum circuit (left)** includes **multi-qubit gates** like **CCX** (Toffoli gates), such as `e4` and `e5`.
- The **hypergraph (right)** captures these **multi-qubit dependencies**, allowing **hyperedges** to connect more than two qubits.

This representation supports **quantum circuit cutting**, enabling **efficient partitioning** across **multiple QPUs**.

---

### 🖼️ [Figure 5: Hypergraph Structure for Circuit Representation](https://github.com/hypaq/hypaq/blob/main/images/fig5.png)

<p align="center">
  <img src="https://github.com/hypaq/hypaq/blob/main/images/fig5.png" alt="Graph vs. Hypergraph" width="70%">
</p>

This figure details the **hypergraph structure** used to represent complex quantum circuits:
- **Nodes** represent **qubits**, while **hyperedges** represent **quantum gates**, including multi-qubit gates.
- The **data structure** (right) shows how nodes and hyperedges are encoded, with additional attributes from quantum gates (e.g., qubit identifiers, gate types).

This **flexible hypergraph model** captures various patterns and complexities from input circuits, forming the foundation for **partitioning** and **distributing circuits** in **multi-QPU systems**.

---

> 💡 **The hypergraph-based approach in HyPAQ enables accurate modeling and cutting of complex quantum circuits**, supporting efficient distribution and execution on **noisy intermediate-scale quantum (NISQ)** devices.

