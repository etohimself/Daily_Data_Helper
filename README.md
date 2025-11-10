# Daily Data Helper

Excel uygulaması olmadan ham veri hatalarını gidermeyi sağlayan basit bir uygulamadır.

---

## 🧰 Gereksinimler
- Windows 10 ve üzeri
- [.NET 8 Runtime or SDK]
- ClosedXML (NuGet ile eklendi, yalnızca derlemek için)

---

## 🧑‍💻 Derleme Adımları

1. **Repository'yi Klonlayın:**
   ```bash
   git clone https://github.com/etohimself/Daily_Data_Helper.git
   cd Daily_Data_Helper
   ```
2. **NuGet Paketlerini Yükleyin:**
   ```bash
   dotnet restore
   ```
3. **Hedef Sistemde .NET Desktop Runtime 8 Yüklü İse:**
   ```bash   
    dotnet publish -c Release -r win-x64 --self-contained false
    ```
4. **Hedef Sistemde .NET Desktop Runtime 8 Yüklü Değil İse (Portable):**
   ```bash   
    dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true
    ```
5. **EXE Dosyasını Bulun:**
   - 3. adımı izleyerek derlediyseniz .exe dosyası şu konumda olacaktır :
   ```bash
   bin\Release\net8.0-windows\win-x64\publish\Daily_Data_Helper.exe
   ```
   - 4. adımı izleyerek portable derlediyseniz .exe dosyası şu konumda olacaktır : 
   ```bash
   \bin\Release\net8.0-windows\win-x64\publish\Daily_Data_Helper.exe
   ```
 

## 📄 Lisans

ClosedXML kütüphanesi kullanıldığı için Apache 2.0 lisansı geçerlidir.

```
Bu proje ClosedXML kütüphanesini kullanmaktadır.
ClosedXML, Apache License 2.0 altında lisanslanmıştır.
```

---