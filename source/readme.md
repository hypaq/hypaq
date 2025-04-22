## ⚙️ HyPAQ Framework Overview

The **HyPAQ** (Hypergraphic Partitioning Approach for Quantum circuits) framework provides a comprehensive set of tools to enable **quantum circuit cutting**—the decomposition and distribution of large quantum circuits across multiple **Quantum Processing Units (QPUs)** in distributed quantum computing environments. By leveraging **hypergraph-based representations**, HyPAQ addresses the challenges of partitioning both **static** and **adaptive** quantum circuits, optimizing their execution on **noisy intermediate-scale quantum (NISQ)** devices.

---

### 🧩 Static Quantum Circuits

For **static quantum circuits**, HyPAQ includes a suite of functions for circuit generation, hypergraph construction, partitioning, and result consolidation:

- **`Full_Circuit_static_generator_v1`**  
  Generates complete static quantum circuits used as input for partitioning and evaluation.

- **Partitioning Heuristics:**  
  - **`classicFM_static_partition_v1`** – Fiduccia-Mattheyses (FM) partitioning  
  - **`classicKL_static_partition_v1`** – Kernighan-Lin (KL) partitioning  
  - **`classicSW_static_partition_v1`** – Stoer-Wagner (SW) partitioning  

- **Hypergraph Construction:**  
  - **`HyPAQ_static_hypergraph_generator_v1`** – Builds hypergraph representations of static circuits.  
  - **`HyPAQ_Incidence_Matrix_Generator_v1`** – Generates incidence matrices for structural analysis.

- **HyPAQ Partitioning Algorithm:**  
  - **`HyPAQ_static_partition_v1`** – Applies HyPAQ’s optimized partitioning strategy on static circuit hypergraphs.

- **Partition Consolidation:**  
  - **`Consolidate_KL_static_partition`**  
  - **`Consolidate_FM_static_partition`**  
  - **`Consolidate_SW_static_partition`**  
  - **`Consolidate_HyPAQ_static_partition`**  
  Aggregates and refines partitioning results across different heuristics.

---

### 🔄 Adaptive Quantum Circuits

For **adaptive quantum circuits**, which include mid-circuit measurements and dynamic control, HyPAQ provides:

- **`HyPAQ_Adaptive_Quantum_Circuit_Generator_v1`**  
  Generates adaptive quantum circuits with dynamic feedback.

- **`HyPAQ_Adaptive_Hypergraph_Generator_v2`**  
  Builds extended hypergraph models capturing adaptive circuit complexities.

- **`HyPAQ_Adaptive_Partition_v1`**  
  Applies partitioning tailored for adaptive hypergraphs, considering real-time circuit adjustments.

---

### 🗂️ Quantum Resource Management (QRM)

To manage quantum resources across distributed systems, HyPAQ integrates **Quantum Resource Management (QRM)** tools:

- **`HyPAQ_QRM_Adding_Pragma_v3`**  
  Adds **pragma directives** for annotating resource boundaries in circuits.

- **`HyPAQ_QRM_Counting_Pragma_Time_v1`**  
  Analyzes and measures the time overhead introduced by pragmas, helping assess communication and scheduling impacts.

- **`HyPAQ_QRM_Quantum_Resource_Manager_v2`**  
  Manages resource allocation and orchestration across QPUs, minimizing inter-QPU communication costs.

---

### 🚀 Summary

The **HyPAQ framework** equips researchers and developers with essential tools for **efficient quantum circuit cutting** and **distributed execution** across **multi-QPU systems**. Supporting both **static** and **adaptive circuits**, HyPAQ enhances scalability and flexibility in quantum algorithm deployment, paving the way for advanced quantum computing on **NISQ devices**.

---

> 💡 **For more details on how to use these functions, check the [Documentation](#) and [Examples](#).**

