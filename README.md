# 🛒 F# Store Simulator

A modern **educational desktop application** built with **F#** and **Avalonia UI** that demonstrates functional programming concepts through a simple store and shopping cart experience.

---

## 📌 Overview

The **F# Store Simulator** is designed for students learning:

* Functional Programming with **F#**
* Immutable data structures (**List**, **Map**)
* Pure functions and predictable state
* MVVM architecture
* Collaborative development using **Git & GitHub**

Users can browse products, manage a cart, apply discounts, and complete checkout while all business logic remains immutable and testable.

---

## ✨ Features

* 📦 Product catalog stored in immutable `Map`
* 🛒 Shopping cart with add/remove/update operations
* 💰 Automatic and coupon-based discounts
* 🔍 Search, filter, and sort products
* 💾 Save completed orders as JSON
* 🖥 Cross-platform GUI using Avalonia UI
* ✅ Extensive unit & integration tests

---

## 🏗 Architecture

The application follows the **MVVM (Model–View–ViewModel)** pattern:

<img width="599" height="494" alt="image" src="https://github.com/user-attachments/assets/86e51bab-11a6-4d5d-bff8-adefad4a40ba" />


All core logic is isolated from the UI to ensure maintainability and testability.

---

## 🧰 Technology Stack

* **Language:** F# (.NET 8.0)
* **UI Framework:** Avalonia UI
* **Architecture:** MVVM
* **Serialization:** Newtonsoft.Json
* **Testing:** xUnit, FsUnit

---

## 📂 Project Structure

<img width="460" height="502" alt="image" src="https://github.com/user-attachments/assets/5d705fae-7f2c-44fd-aec6-131e54b81863" />


---

## ▶️ Getting Started

### Prerequisites

* .NET 8.0 SDK
* Visual Studio / VS Code / Rider

### Run the Application

```bash
git clone https://github.com/youssef2004wael/StoreSimulatorGUI.git
cd StoreSimulatorGUI
dotnet restore
dotnet run
```

### Run Tests

```bash
dotnet test
```

---

## 🧪 Test Coverage

* 90+ automated tests
* Unit tests for each module
* Integration tests for full workflows

Testing ensures correctness of cart logic, pricing, search, and persistence.

---

## 👥 Team Roles

* Catalog Developer
* Cart Logic Developer
* Price Calculator Developer
* Search & Filter Developer
* File Save/Load Developer
* UI Developer
* Tester
* **GitHub / Documentation Lead**

---

## 🎓 Educational Value

This project serves as a **complete academic example** of applying functional programming principles in a real-world-style desktop application.

It is suitable for:

* University coursework
* Functional programming practice
* MVVM architecture learning

---

## 📄 License

This project is intended for **educational purposes** only.

---

⭐ If you find this project useful, feel free to star the repository!
