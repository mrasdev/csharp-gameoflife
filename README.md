# High-Performance Cellular Automata Engine (.NET 9)

A highly optimized, multi-threaded, and **zero-allocation** simulation engine for *Conway's Game of Life* and *HighLife*, built with **.NET 9**. This project leverages low-level optimization techniques, modern C# features, and hardware awareness to maximize execution speed and throughput on modern multi-core CPUs.

![Screenshot](Screenshot.png "Screenshot")

---

## Performance & Architecture Highlights

This engine was architected from the ground up to eliminate common managed-runtime overheads:

* **Zero Allocations (0 Bytes Heap Allocation):** Once the initialization phase is complete, both the main simulation loop and the rendering pipeline perform absolutely zero heap allocations. This completely eliminates Garbage Collection (GC) latency and pauses, ensuring rock-solid, fluid execution.
* **Dedicated Worker Threads & Zero-Alloc Threading:** Instead of relying on the standard .NET ThreadPool or `Parallel.For` (which incur task-scheduling overhead, object allocations, and closure-based latency), the engine spins up permanent, dedicated background threads matching `Environment.ProcessorCount`. These threads remain alive for the application's entire lifecycle, eliminating thread creation costs during runtime.
* **Low-Level Barrier Synchronization:** Precise coordination between the orchestrating simulation thread and the active computing threads is achieved via a hardware-friendly `System.Threading.Barrier` structure. Operating in a lightweight two-phase cycle (Start/Finish), it blocks idle threads at the kernel level without wasting CPU cycles or generating heap debris.
* **Compile-Time Polymorphism via Struct Constraints:** Instead of using traditional object-oriented interfaces (`ICellularRule` and `INeighbourhoodStrategy`) which incur virtual method invocation overhead (vtable lookups), rules and neighborhood typologies are implemented as **`structs`**. By passing them into the core engine via generic constraints (`where TRule : struct`), the .NET JIT compiler performs **Aggressive Inlining**, embedding the logic directly into the processing loop.
* **Branchless Logical Execution:** The state transitions for cells (`ConwayRule` and `HighLifeRule`) are computed using mathematical bitwise operations (`BornMask`, `SurviveMask`, `XorMask`) rather than conditional jumps (`if-else`). This eliminates CPU branch mispredictions, maintaining a highly deterministic execution pipeline regardless of population chaos.
* **Double Buffering via Pointer Swap:** The grid is represented as a contiguous, flat 1D array wrapped in a specialized `GridBuffer`. To compute the next generation, the engine reads from the current buffer and writes to the next. At the end of a cycle, a high-speed reference swap occurs without copying underlying array elements.
* **Cache Locality & Thread-Safe Chunking:** The grid is partitioned statically into distinct, continuous row blocks (chunks) assigned to individual worker threads. Threads accumulate cell changes onto local variables inside their private CPU stack framework, completely neutralizing the performance-destroying effects of *False Sharing* in the L1/L2 caches.
* **Allocation-Free Text & Graphics Streaming:** The rendering subsystem bypasses all `System.String` and `StringBuilder` allocations. It utilizes a persistent, raw `char[]` buffer for the entire viewport and formats dynamic numeric HUD metrics directly onto the CPU stack using `ISpanFormattable.TryFormat` and `Span<char>`. The final output is written to the terminal as a unified stream via a direct `ReadOnlySpan<char>` dump, paired with ANSI escape codes for rapid, completely flicker-free visuals.


---

## Features

* **Multiple Rulesets:** Supports classic *Conway's Game of Life* (B3/S23) and *HighLife* (B36/S23), which features native replicator patterns.
* **Multiple Neighborhoods:** Toggle between *Moore Neighborhood* (8 surrounding cells) and *Von Neumann Neighborhood* (4 orthogonal cells).
* **Flexible Topologies:** Configure the board as a bounded finite grid or a *Toroidal grid* (seamless wrapping around edges).
* **RLE Pattern Parser:** Built-in support to parse and load standard `.rle` (Run-Length Encoded) files directly onto the grid, handling positioning and resizing dynamically.
* **Real-time Performance HUD:** Tracks and displays Generations, Living Cells, Grids/sec (Updates Per Second), Threads/sec, and microscopic Cell Checks/sec with textbook right-aligned padding.

---

## Configuration (`settings.json`)

The application automatically loads or generates a default configuration file on startup:

```json
{
  "Width": 1000,
  "Height": 1000,
  "Toroidal": true,
  "RuleType": "Conway",
  "NeighbourType": "Moore",
  "UseRandomPattern": true,
  "Density": 0.3,
  "RlePath": "patterns/gosper_glider_gun.rle",
  "FpsRate": 10,
  "StartupMode": "Fast",
  "ShowHelpScreen": true
}