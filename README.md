<p align="center">
  <img src="ChronoViewLogo.png" alt="ChronoView Logo" width="200"/>
</p>

<h1 align="center">ChronoView </br></br>

<img src="https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white"> <!--<img src="https://img.shields.io/badge/Linux-FCC624?style=for-the-badge&logo=linux&logoColor=black"> <img src="https://img.shields.io/badge/mac%20os-000000?style=for-the-badge&logo=macos&logoColor=F0F0F0"> 
-->
<img src="https://img.shields.io/badge/-.NET%2010.0-blueviolet">
[![Azure Static Web Apps CI/CD](https://github.com/4dillusions/ChronoView/actions/workflows/dotnet.yml/badge.svg)](https://github.com/4dillusions/ChronoView/actions/workflows/dotnet-desktop.yml)

</h1>

ChronoView is a minimalist photo timeline viewer built with **WinUI 3** and **C# (MVVM pattern)**.  
It lets you explore your images through time — smooth, responsive, and focused on clean UI and intuitive interaction.

---

## 🕓 Overview

ChronoView visualizes JPEG images from a selected folder along a horizontal timeline.  
Each photo is positioned based on its creation date, allowing you to scroll or zoom through your visual history.  
The main viewer dynamically updates to display the image corresponding to the center point of the timeline.

---

## ✨ Features

- 📁 **Folder-based photo loading** — automatically scans and loads `.jpg` / `.jpeg` files  
- 🖼️ **Timeline view** — displays each image according to its timestamp  
- 🔍 **Zoom & Pan** — intuitive timeline navigation  
- 🪄 **Smooth transitions** — optional animations for zoom and image changes  
- 💬 **Hover previews** — thumbnail tooltips (optional extra)  
- ⏯️ **Slideshow mode** — play through images as a continuous sequence  

---

## 🧩 Tech Stack

- **Language:** C#  
- **Framework:** WinUI 3  
- **Architecture:** MVVM (Model–View–ViewModel)  
- **UI/UX:** Responsive layout, touch & mouse support  
- **Async loading:** optional, for handling large image sets  

---

## 🧠 Concept

ChronoView is a personal exploration of **temporal storytelling through images**.  
It combines simple data binding and reactive UI concepts with a focus on **clean design and user flow**.  
It also serves as a small demonstration of **WinUI + MVVM structure** in a modern desktop context.

---

## 📥 Clone
Clone the entire project including the submodules:<br>
```bash
git clone --recurse-submodules https://github.com/4dillusions/ChronoView.git
```

If the project is already cloned and you forgot to fetch the submodules:<br>
```bash
git submodule update --init --recursive
```

If the submodules have been updated and you want to fetch the latest changes:<br>
```bash
git submodule update --remote --merge
```
