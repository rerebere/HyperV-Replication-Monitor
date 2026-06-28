# Hyper-V Replication Monitor

Egy felhasználóbarát Windows alkalmazás két Hyper-V szerver kereszt-replikációjának monitorozásához és automatikus failover kezeléshez.

## Jellemzők

- ✅ Valós idejű replikációs állapot monitorozása
- ✅ Grafikus felhasználói felület (WPF)
- ✅ Kézi failover kezelés
- ✅ Titkosított jelszókezelés
- ✅ Részletes naplózás (3 napig őrzött)
- ✅ Előzmények megjelenítése
- ✅ 1 perces frissítési ciklus

## Szerverek

- **HMHUVG21MP01**: 10.8.248.40
- **HMHUVG21MP02**: 10.8.248.41

## Rendszerkövetelmények

- Windows 11+
- .NET Framework 4.7.2+
- Visual Studio 2022+
- Hyper-V WMI hozzáférés

## Telepítés

1. Klónozd a repo-t:
   ```bash
   git clone https://github.com/rerebere/HyperV-Replication-Monitor.git
   ```

2. Nyisd meg a `HyperVMonitor.sln` fájlt Visual Studio-ban

3. Fordítsd össze (Build > Build Solution)

4. Futtasd a projektet (F5)

## Használat

1. Indítsd el az alkalmazást
2. Add meg a felhasználóneveket és jelszavakat mindkét szerverhez
3. Az alkalmazás automatikusan monitorozza a replikáció állapotát
4. Kattints a "Failover" gombra, ha szükséges
5. Nézd meg az előzményeket az "Előzmények" fülön

## Naplózás

A naplók tárolása: `C:\HyperV-Monitor\Logs\`

## Fejlesztés

- **Language**: C# / WPF
- **Visual Studio**: 2022+
- **.NET Target**: .NET Framework 4.7.2+

## License

MIT
