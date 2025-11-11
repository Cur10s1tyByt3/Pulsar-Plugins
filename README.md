# 🔌 Pulsar Plugins

This repository contains official and community plugin projects for the **Pulsar** ecosystem.

Plugins extend the functionality of the Pulsar client and server by using shared code from [`Pulsar.Common`](https://github.com/Quasar-Continuation/Poopsar/tree/main/Pulsar.Common).

---

## ⚙️ Setup Instructions

### 🧩 1. Clone the repository with submodules

> `Pulsar.Common` is required for all plugin builds.  
> Clone using the command below to automatically include it.

```bash
git clone --recurse-submodules https://github.com/Quasar-Continuation/Pulsar-Plugins.git
If you already cloned it normally, just run:

bash
Copy code
git submodule update --init --recursive
```

### 🔄 Updating to the Latest Pulsar.Common

To update your local copy to the newest version from the Poopsar repository:

```bash
git submodule update --remote --merge
```