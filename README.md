<div align="center">
  <img src="hypaq.png" alt="HyPAQ" width="100"/>
  <br/>
  <strong>HyPAQ</strong>
</div>

# HyPAQ - Hypergraph Partitioning for Static and Adaptive Quantum Circuits
[![PyPI downloads](https://img.shields.io/pypi/dm/your-package-name?label=PyPI%20downloads)](https://pypi.org/project/hypaq/)
[![Stack Overflow](https://img.shields.io/badge/stackoverflow-Ask%20questions-blue)](https://stackoverflow.com/questions/tagged/hypaq)
[![DOI](https://img.shields.io/badge/DOI-10.1109%2FQCE57702.2023.00055-blue)](https://doi.org/10.48550/arXiv.2504.09318)
[![Contribute](https://img.shields.io/badge/Contribute-Good%20First%20Issue-brightgreen)](https://github.com/hypaq/hypaq/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22)

**HyPAQ**, an acronym for Hypergraphic Partitioning Approach for Quantum circuits, is a powerful research-driven initiative aimed at addressing key scalability and efficiency challenges in distributed quantum computing. At its core, HyPAQ focuses on the development of advanced partitioning strategies for both static and adaptive quantum circuits by leveraging hypergraph-based representations. These representations enable a more precise modeling of quantum gate dependencies and classical control operations, which are particularly complex in adaptive circuits. The main goal of HyPAQ is to optimize the distribution of quantum computations across multiple Quantum Processing Units (QPUs), reducing inter-QPU communication costs and enhancing overall computational performance. By introducing standardized benchmarks and heuristics tailored for multi-QPU systems, HyPAQ provides a foundational framework for evaluating and improving partitioning techniques—ultimately facilitating more scalable, efficient, and flexible quantum algorithm execution in the era of noisy intermediate-scale quantum (NISQ) devices.

# HyPAQ Methodology
<p align="center">
  <img src="https://github.com/hypaq/hypaq/blob/main/images/hypaq%20methodology.png" alt="Graph vs. Hypergraph" width="70%">
</p>

## 🧩 HyPAQ Methodology: Quantum Circuit Cutting Using Hypergraphic Heuristics

The **HyPAQ (Hypergraphic Partitioning Approach for Quantum circuits)** methodology provides a structured approach for **quantum circuit cutting** in distributed quantum computing environments. By leveraging **hypergraph-based representations** and advanced **partitioning heuristics**, HyPAQ enables efficient execution of quantum circuits across **multiple Quantum Processing Units (QPUs)**.

---

### 📊 Workflow Overview

1. ### **Input Quantum Circuit**

   - Starts with a **quantum circuit** (e.g., in QASM format).
   - Comprises **qubits** and **quantum gates**, including multi-qubit gates like **CX** or **CCX**.

2. ### **Hypergraph Construction**

   - Converts the circuit into a **hypergraph**:
     - **Nodes** represent **qubits**.
     - **Hyperedges** represent **gates** (connecting two or more qubits).
   - Captures **multi-qubit dependencies** naturally (e.g., CCX gates connect three qubits).

3. ### **Hypergraphic Partitioning Heuristics**

   - Applies **partitioning heuristics**:
     - **Fiduccia-Mattheyses (FM)**, **Kernighan-Lin (KL)**, or **HyPAQ’s custom strategies**.
   - **Objective**:
     - Minimize **inter-QPU communication**.
     - Balance **workload distribution**.
     - Preserve **control dependencies**.

4. ### **Partitioned Hypergraph → Subcircuits**

   - Maps partitions back to **subcircuits**:
     - Assigns gates/qubits to specific **QPUs**.
     - Introduces **cut points** at **cross-partition hyperedges**.

5. ### **Distributed Quantum Execution**

   - Executes **subcircuits** across **multiple QPUs**.
   - Managed by the **Quantum Resource Manager (QRM)**:
     - Inserts **pragma directives** for synchronization.
     - Oversees **communication costs** and **execution timing**.

6. ### **Output: Optimized Distributed Execution**

   - Achieves **scalable** and **efficient execution** across **multi-QPU systems**.
   - Addresses the limitations of **NISQ (Noisy Intermediate-Scale Quantum)** devices.

---

### 🚀 Key Benefits of HyPAQ’s Hypergraphic Approach

- Models **multi-qubit dependencies** accurately.
- Supports both **static** and **adaptive** quantum circuits.
- Minimizes **inter-QPU communication overhead**.
- Enables **scalability** in **distributed quantum computing**.

By leveraging **hypergraphic heuristics**, the **HyPAQ framework** facilitates practical **quantum circuit cutting**, paving the way for efficient **multi-QPU quantum computing**.

---

> 💡 **For more details, see the [Documentation](#) and [Examples](#).**

#
updated: April 2025.
