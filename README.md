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

The **HyPAQ (Hypergraphic Partitioning Approach for Quantum circuits)** methodology introduces a robust framework for **quantum circuit cutting** in **distributed quantum computing environments**. By utilizing **hypergraph-based representations** and sophisticated **partitioning heuristics**, HyPAQ enables efficient distribution of quantum circuits across **multiple Quantum Processing Units (QPUs)**. This methodology addresses the scalability challenges faced by **noisy intermediate-scale quantum (NISQ)** devices, providing a pathway toward large-scale quantum computation.

---

### 📊 Workflow Overview

The HyPAQ methodology is composed of several key stages that transform a quantum circuit into a partitioned structure suitable for distributed execution across multiple QPUs. Below is an overview of each stage in the process.

---

### 1. **Input Quantum Circuit**

The process begins with a standard **quantum circuit**, which can be provided in formats like **QASM** or as a **Quantum DAG** (Directed Acyclic Graph). These circuits define the **qubits** involved and the **quantum gates** applied to them. The gates can range from single-qubit operations to multi-qubit gates such as **CX** (CNOT) or **CCX** (Toffoli), which introduce complex dependencies between multiple qubits.

- Includes:
  - **Qubits** and their connections.
  - **Quantum gates**, including multi-qubit interactions.

---

### 2. **Hypergraph Construction**

To effectively model the dependencies within a quantum circuit, HyPAQ transforms the circuit into a **hypergraph**. Unlike standard graphs, where edges connect two nodes, a **hypergraph** allows **hyperedges** that can connect **multiple nodes simultaneously**, capturing complex **multi-qubit interactions** inherent in quantum circuits.

- **Nodes** represent **qubits**.
- **Hyperedges** represent **quantum gates** (e.g., a **CCX** gate becomes a hyperedge connecting three qubits).
- This model allows **non-binary relationships** to be captured, essential for accurate circuit representation.

---

### 3. **Hypergraphic Partitioning Heuristics**

Once the hypergraph is constructed, HyPAQ applies advanced **partitioning heuristics** to divide the hypergraph into segments that can be executed independently on different QPUs. The goal of these heuristics is to **minimize the communication overhead** between QPUs by reducing the number of **hyperedges crossing partitions** (the "cuts" in the circuit), while maintaining **balanced workloads** across the system.

- **Partitioning strategies** include:
  - **Fiduccia-Mattheyses (FM)** heuristic.
  - **Kernighan-Lin (KL)** algorithm.
  - Custom **HyPAQ heuristics** optimized for quantum circuits.
- Ensures:
  - **Balanced QPU workloads**.
  - Preservation of **gate dependencies**.
  - **Minimized inter-QPU communication**.

---

### 4. **Partitioned Hypergraph → Subcircuits**

Following partitioning, the hypergraph is **mapped back** to the **quantum circuit domain** by generating **subcircuits** for each QPU. These subcircuits contain the necessary qubits and gates for local execution, and **cut points** are inserted at **cross-partition hyperedges**, indicating where **inter-QPU communication** is needed.

- **Subcircuits** correspond to **QPU workloads**.
- **Cut points** mark **communication boundaries** between QPUs.

---

### 5. **Distributed Quantum Execution**

The resulting subcircuits are executed across **multiple QPUs** in a distributed environment. The **Quantum Resource Manager (QRM)** oversees the execution process, handling **synchronization** and **communication** between QPUs. It introduces **pragma directives** that coordinate the execution flow, ensuring the **correct sequencing** and **data sharing** between subcircuits.

- **QRM responsibilities**:
  - Insert **pragma directives** for synchronization.
  - Manage **inter-QPU communication timing** and **costs**.

---

### 6. **Output: Optimized Distributed Execution**

At the conclusion of this process, the quantum circuit has been effectively **cut** and distributed for **optimized execution** across **multi-QPU systems**. This method mitigates the limitations of **NISQ devices**, providing a scalable approach to complex quantum computations.

- **Results**:
  - **Efficient execution** across multiple QPUs.
  - **Reduced communication overhead**.
  - **Enhanced scalability** for large quantum circuits.

---

### 🚀 Key Benefits of HyPAQ’s Hypergraphic Approach

The **HyPAQ framework** delivers significant advantages through its **hypergraph-based methodology**:
- **Accurate modeling** of complex, **multi-qubit dependencies**.
- Supports both **static** and **adaptive** quantum circuits.
- **Minimizes communication overhead** between QPUs.
- **Scales** quantum workloads for distributed environments, extending the capabilities of **NISQ devices**.

By enabling practical **quantum circuit cutting**, HyPAQ contributes to the advancement of **distributed quantum computing**, paving the way for scalable, high-performance quantum algorithms.

---

> 💡 **For more technical details, see the [Documentation](#) and explore [Examples](https://github.com/hypaq/hypaq/tree/main/examples) of HyPAQ in action.**
updated: April 2025.
