# Spline — Sistem Dokümantasyonu

Bu doküman, projedeki tüm C# sistemlerini, aralarındaki ilişkiyi ve çalışma mantığını detaylı biçimde açıklar. Amaç: kod tabanına yeni bakan birinin (veya ileride kendinizin) hızlıca "bu proje ne yapıyor, parçalar nasıl bağlanıyor" sorusuna cevap bulabilmesi.

- **Unity sürümü:** 6000.3.11f1 (Unity 6)
- **Ana namespace:** `Wonnasmith.Spline` (editör yardımcıları `WonnasmithEditor` namespace'inde)
- **Proje kök klasörü:** `Spline/Assets`

---

## 1. Genel Bakış

Proje, Unity Editor içinde **elle (manuel) node/nokta ekleyerek Bézier eğrisi (spline) oluşturmayı** ve bu eğri üzerinden:

1. Görsel debug çizimleri (Gizmos) yapmayı,
2. Eğri şeklinde bir **mesh (yol/şerit) üretmeyi**,
3. Bir objeyi bu eğri üzerinde **hareket ettirmeyi (follower)**

sağlayan, sıfırdan yazılmış küçük bir spline aracıdır. Unity'nin resmi Splines paketi kullanılmıyor; matematik (Bernstein/Bézier hesaplama) dahil her şey proje içinde custom yazılmış.

### Klasör yapısı

```
Assets/
├── Attribute/              # Custom Inspector attribute tanımları
│   ├── ButtonAttribute.cs
│   └── HelpBoxAttribute.cs
├── Editor/                 # Yukarıdaki attribute'ları Inspector'da çizen drawer'lar
│   ├── ButtonDrawer.cs
│   └── HelpBoxAttributeDrawer.cs
├── Resources/               # Runtime'da Resources.Load ile yüklenen asset'ler
│   ├── NodePrefab/NODE.prefab
│   └── SplineMeshMaterial/DefaultSplineMaterial.mat
└── _Game/
    ├── Scenes/
    │   ├── Spline.unity          # Sadece gizmo/nokta tabanlı spline test sahnesi
    │   └── MeshSpline.unity      # Mesh üretimini test eden sahne
    └── Scripts/
        ├── Base/SplineBase.cs        # Tüm sistemin kalbi (abstract)
        ├── Spline.cs                 # SplineBase'in gizmo-only implementasyonu
        ├── SplineMesh.cs             # SplineBase'in mesh üreten implementasyonu
        ├── NodeController.cs         # Her bir spline noktasının davranışı
        ├── SplineFollower.cs         # Bir objeyi spline üzerinde hareket ettirir
        └── Extensions/
            ├── WonnaMathf.cs         # Bernstein/Bézier matematiği
            └── WonnaExtensions.cs    # Genel amaçlı C#/Unity extension metodları
```

---

## 2. Çekirdek Sistem: `SplineBase` (`Base/SplineBase.cs`)

`SplineBase`, `MonoBehaviour`'dan türeyen **abstract** bir sınıf. `[ExecuteInEditMode]` ile işaretlenmiş, yani Play Mode'a girmeden Editor'da da çalışır — bu tamamen bir **editör-zamanı düzenleme aracı** olduğu için kritik bir detay.

Somut iki alt sınıfı var: `Spline` (sadece gizmo çizer) ve `SplineMesh` (ayrıca mesh üretir). İkisi de davranışın tamamını `SplineBase`'den miras alır.

### 2.1 Node yönetimi

- `_nodeList : List<NodeController>` — spline'ı oluşturan noktaların (child GameObject) referansları.
- `_posList : List<Vector3>` — bu node'lardan Bézier formülüyle **örneklenmiş** ara pozisyonlar (gizmo çizimi ve mesh üretimi bunları kullanır, ham node pozisyonlarını değil).

**`NodeGenerator()`** (Inspector'da bir buton olarak gösterilir, bkz. §4):
- `Resources/NodePrefab/NODE` prefab'ını `Resources.Load` ile yükler (ilk çağrıda cache'ler).
- Yeni bir `NODE_` GameObject'i instantiate edip spline'ın transform'una child olarak ekler, `_nodeList`'e ekler.
- `AllNodeRename()` ile tüm node'ları `NODE_0`, `NODE_1`, ... şeklinde yeniden adlandırır.

**`NodeClear()`**: tüm node GameObject'lerini `DestroyImmediate` ile siler, listeleri temizler, spline'ın kendi transform'unu `Vector3.zero`'a resetler.

**Node silme akışı (event-driven):**
- `NodeController` üzerinde her node'un kendi "sil" butonu var; tıklanınca `NodeController.NodeDeleteButtonClick` **static event**'ini tetikler.
- `SplineBase.OnEnable()` bu event'e abone olur (`OnNodeDeleteButtonClick`), `OnDisable()`'da abonelikten çıkar.
- Handler, ilgili node'u `_nodeList`'ten çıkarıp `DestroyImmediate` ile yok eder ve yeniden adlandırır.
- Bu tasarımın önemli sonucu: **sahnede birden fazla spline objesi olsa bile event static olduğu için her spline kendi node'unun silinip silinmediğini `_nodeList.Contains()` kontrolüyle ayırt eder.**
- `EditorApplication.isPlaying` kontrolü her yerde var — bu araç sadece Edit Mode'da çalışacak şekilde tasarlanmış.

### 2.2 Eğri matematiği — Bézier / Bernstein

**`BernsteinPositionCalculator(float percent)`**: verilen `t` (0-1 arası) değeri için, **tüm node'ları kontrol noktası kabul eden tek bir yüksek dereceli Bézier eğrisi** üzerindeki pozisyonu hesaplar (klasik De Casteljau/segment tabanlı Bézier değil — `n = nodeCount - 1` dereceli genel Bézier formülü).

```
B(t) = Σ P_i * Bernstein(n, i, t)   for i in 0..n
```

Formülün kendisi `Extensions/WonnaMathf.cs` içinde:
- `WonnaBinom(upper, lower)` → binom katsayısı, hazır bir `Factorial` lookup tablosu (17 eleman, 16!'ya kadar) kullanıyor.
- `WonnaBernstein(n, v, t)` → `C(n,v) * t^v * (1-t)^(n-v)`.

> ⚠️ **Ölçeklenme sınırı:** `Factorial` dizisi sadece 17 elemanlı (0!–16!). Yani **17'den fazla node eklenirse** `WonnaBinom` bir `IndexOutOfRangeException` fırlatır. Pratikte bu, spline'a eklenebilecek node sayısını ~16 ile sınırlıyor. Çok node'lu eğriler istenirse mimarinin segment bazlı (parçalı Bézier / Catmull-Rom) bir yaklaşıma geçmesi gerekir.

**`PositionListUpdate()`**: `_posList`'i her `OnDrawGizmos` çağrısında yeniden hesaplar (yani sürekli, her frame/gizmo redraw'da — cache'lenmiyor).
- `pointCount` Inspector alanına göre `t` parametre aralığını örnekler (`splinePercentRate = t / (pointCount - 1)`).
- `t` (0-1) Inspector'daki slider'dan gelir ve **eğrinin ne kadarının çizileceğini/üretileceğini** kontrol eder — yani `t < 1` verilirse eğrinin sadece bir kısmı örneklenir.

**`GetPointTangent(p1, p2)`**: iki nokta arasındaki yön vektörünü normalize edip Y ekseni etrafında 90° döndürerek **o segmente dik (perpendicular) bir vektör** üretir. Bu, hem tangent gizmo çiziminde hem de mesh genişliği hesaplamasında (bkz. §3) kullanılıyor.

### 2.3 Gizmo çizimleri (`OnDrawGizmos`)

Sırasıyla:
1. `DrawNodePoint()` — sarı küreler, ham node pozisyonları.
2. `DrawLineTest()` — beyaz çizgiler, node'lar arası düz bağlantı (referans amaçlı, gerçek eğri değil).
3. `PositionListUpdate()` — `_posList`'i günceller.
4. `DrawPointTest()` — kırmızı küreler, örneklenmiş Bézier noktaları.
5. `DrawPointTangent()` — mavi çizgiler, her örnek noktadaki tangent/genişlik göstergesi (`tangentGizmoLength` ile ölçeklenir).

### 2.4 Inspector'a özel iki alan

- `pointCount` (`[Min(1)]`) — eğrinin kaç ara noktayla örnekleneceği.
- `t` (`[Range(0,1)]`) — eğrinin ne kadarının hesaplanacağı/gösterileceği.
- `tangentGizmoLength` — sadece görsel debug uzunluğu, mesh'i etkilemez.

---

## 3. `SplineMesh` (`SplineMesh.cs`) — Yol/Şerit Mesh Üretimi

`SplineBase`'i extend eder, ayrıca `[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]` taşır.

**Akış:**
1. Kullanıcı Inspector'daki **"MeshRebuild" butonuna** basar (bkz. §4) → `MeshRebuild()` → `MeshGenerator()`.
2. `MeshMaterialEdit()`: `MeshRenderer.material` boşsa `Resources/SplineMeshMaterial/DefaultSplineMaterial` yüklenip atanır (lazy, sadece bir kez).
3. `_posList` (yani `SplineBase`'in ürettiği örneklenmiş Bézier noktaları) üzerinden, **her nokta için 2 vertex** üretilir: noktadan `±width/2` kadar, o noktadaki tangent'e dik yönde kaydırılmış iki nokta. Sonuç: bir "şerit/yol" mesh'inin sol-sağ kenar vertex'leri.
4. Üçgenler (`_triangles`), vertex dizisi üzerinde **zigzag (triangle strip benzeri) bir indeksleme** ile elle hesaplanıyor (döngü, tersten `_triangles.Length - 1`'den başlayıp geriye doğru gidiyor, `isTurnNumOdd` ile tek/çift üçgen arasında geçiş yapıyor).
5. Mesh **tek yüzeyli (single-sided)** — kod içindeki `HelpBox` uyarısı bunu açıkça belirtiyor: node sırasına bağlı olarak normal yönü ters çıkabilir (mesh arkadan bakınca görünmez olabilir).

> Not: `MeshGenerator`, `_mesh.RecalculateNormals()` veya `RecalculateBounds()` çağırmıyor — sadece `vertices` ve `triangles` set ediliyor. UV de üretilmiyor, yani `DefaultSplineMaterial` düz/unlit bir materyal olmalı; texture mapping yapılmaya çalışılırsa sonuç tanımsız olur.

---

## 4. Custom Inspector Sistemi (`Attribute/` + `Editor/`)

Projede iki adet reflection tabanlı custom `PropertyAttribute` var; ikisi de Unity'nin resmi "Inspector Button" ve "HelpBox" community pattern'lerinin bu proje için yazılmış versiyonları.

### `ButtonAttribute` + `ButtonDrawer`
- Kullanım: `[Button(nameof(MethodName))] public bool someBoolField;`
- `ButtonDrawer` (bir `PropertyDrawer`), bu bool alanın yerine bir **buton** çizer; butona tıklanınca reflection ile (`type.GetMethod(methodName)`) ilgili public, parametresiz metodu bulup `Invoke` eder.
- Metot bulunamazsa veya parametre alıyorsa Inspector'da hata mesajı gösterilir (crash yerine güvenli fallback).
- Projede kullanıldığı yerler: `SplineBase.NodeGenerator`, `SplineBase.NodeClear`, `SplineMesh.MeshRebuild`, `NodeController.NodeDeleteButton`.

### `HelpBoxAttribute` + `HelpBoxAttributeDrawer`
- Kullanım: `[HelpBox("mesaj", HelpBoxMessageType.Info)]`
- Bir `DecoratorDrawer` olarak çalışır (property'ye bağlı değil, sadece bir alanın üstüne bilgi kutusu ekler).
- `HelpBoxMessageType` (`None/Info/Warning/Error`) Unity'nin kendi `MessageType` enum'una map'leniyor.
- Yükseklik hesaplaması dinamik: metin uzunluğuna göre `helpBoxStyle.CalcHeight` ile otomatik büyüyor.

Bu ikili, projenin Inspector'ını (ör. `SplineBase` üstündeki "Yeni node oluştur" / "Node'ları sil" açıklamaları) kod yazmadan Inspector'da buton+açıklama çiftleri olarak sunmayı sağlıyor.

---

## 5. `NodeController.cs` — Tek Bir Spline Noktası

Çok küçük ama önemli bir sınıf:
- `NodeDeleteButtonClick` adında **static event** tanımlar (tipi `SplineBase.SplineBaseNodeDeleteButtonClick delegate`).
- Inspector'da `[Button]` ile bir "sil" butonu gösterir; tıklanınca event'i `this` ile invoke eder.
- Kendi başına silme mantığı yok — silme işini dinleyen `SplineBase` yapıyor (bkz. §2.1). Bu, **node'un kendi parent'ını bilmesine gerek kalmadan** silme isteğini yukarı "duyurması" (event/pub-sub) için tercih edilmiş bir tasarım.

---

## 6. `SplineFollower.cs` — Spline Üzerinde Hareket

En basit script. `Update()` içinde:
- `isMove == true` ise, `current_t` değerini `moveSpeed * Time.deltaTime` kadar `1f`'e doğru `MoveTowards` ile artırır (0'dan başlayıp asla resetlenmez — spline sonuna varınca `Clamp01` ile 1'de kilitlenir kalır, **loop yok**).
- Her frame `spline.BernsteinPositionCalculator(current_t)` çağırarak objenin pozisyonunu doğrudan eğri üzerine set eder (fizik/rigidbody yok, transform'u direkt yazıyor).
- `spline` alanı `SplineBase` tipinde — yani hem düz `Spline` hem `SplineMesh` component'i buraya atanabilir (polymorphism).

> Not: `current_t` hiçbir yerde sıfırlanmıyor/yön değiştirmiyor; geri sarma, ping-pong veya loop davranışı istenirse bu script'e eklenmesi gerekir.

---

## 7. `Extensions/WonnaExtensions.cs` — Genel Yardımcı Kütüphane

Spline sistemine özgü değil, projeye dahil edilmiş **genel amaçlı bir Unity/C# extension metod koleksiyonu** (muhtemelen yazarın kişisel "Wonna" kütüphanesi — projeler arası taşınan bir utility dosyası). Region'lara göre:

| Region | İçerik |
|---|---|
| `List<T>` | `Shuffle()` (Fisher-Yates), `GetClone()` |
| `T[]` | `FindArrayIndex()`, `Shuffle()` |
| `Transform` | `TransformCopy()` — position/scale/rotation kopyalar |
| `GameObject`/`Transform` | `SetActiveNullCheck()` — null-safe `SetActive` |
| `Class objeleri` | `CreateDeepCopy<T>()` — **`BinaryFormatter` ile serialize/deserialize** |
| `LayerMask` | `LayerMask2Int()` |
| `Float` | `FloatRemap()`, `Percent2FloatValue()`, `FloatValue2Percent()` |
| `Color` | `ColorPercentRate()` — iki renk arası interpolasyon |
| `String` | `StringSplit()`, `Slice()`, `StringCovenverter()` (bold/italic/underline rich text tag'leri ekler) |
| `Enum` | `EnumCount<T>()` |
| `Toggle` | `GetActiveToggle()` |
| `Geometrik` | `CircleInRandomPosition()`, `GetMidPoint()`, `PointSimetric()`, `PointCircleSimetric()`, `TargetAngleDistance()` |

> ⚠️ **Dikkat — `CreateDeepCopy<T>`:** `System.Runtime.Serialization.Formatters.Binary.BinaryFormatter` kullanıyor. Bu API **.NET tarafından güvenlik nedeniyle deprecated edildi** (CVE geçmişi olan, güvenilmeyen veri deserialize edildiğinde RCE riski taşıyan bir API) ve yeni .NET sürümlerinde tamamen kaldırılıyor/exception fırlatıyor. Ayrıca hedef tip `[Serializable]` olmalı — Unity `MonoBehaviour`/`Component` tiplerinde çalışmaz. Bu metot şu an spline sisteminin hiçbir yerinde çağrılmıyor (kullanılmayan/miras kalmış kod), ama ileride kullanılacaksa değiştirilmesi (örn. `JsonUtility` veya manuel kopyalama) önerilir.

> `PointCircleSimetric` metod ismiyle davranışı örtüşmüyor gibi görünüyor: sadece `point * cos(angle)` döndürüyor, gerçek bir "çember üzerinde simetri/rotasyon" işlemi yapmıyor (bir rotasyon matrisi/`Quaternion` beklenirdi). Kullanılmıyor olması riski düşük tutuyor ama isim ile implementasyon tutarsız.

---

## 8. Uçtan Uca Akış Özeti

```
Kullanıcı Inspector'da "Node Oluştur" butonuna basar
        │
        ▼
SplineBase.NodeGenerator() → NODE prefab instantiate → _nodeList'e eklenir
        │
        ▼ (her Gizmo çiziminde)
SplineBase.OnDrawGizmos()
        │
        ├─ PositionListUpdate() → WonnaMathf.WonnaBernstein() ile _posList hesaplanır
        │
        └─ Gizmo çizimleri (node, çizgi, örnek nokta, tangent)

[SplineMesh kullanılıyorsa]
Kullanıcı "MeshRebuild" butonuna basar
        │
        ▼
SplineMesh.MeshGenerator() → _posList + GetPointTangent() → vertices/triangles → Mesh

[SplineFollower kullanılıyorsa, Play Mode'da]
Update() → current_t ilerler → spline.BernsteinPositionCalculator(current_t) → transform.position
```

---

## 9. Bilinen Kısıtlar / İyileştirme Alanları

1. **Node limiti ~16** — `WonnaMathf.Factorial` tablosunun boyutu (§2.2).
2. **Tek yüzeyli mesh, normal/UV yok** — `SplineMesh` (§3).
3. **`SplineFollower` loop desteklemiyor** — eğri sonunda kalıcı olarak durur (§6).
4. **`_posList` her gizmo çiziminde yeniden hesaplanıyor**, cache yok — çok node'lu/yüksek `pointCount` durumlarında Editor'da performans maliyeti oluşturabilir.
5. **`CreateDeepCopy<T>` deprecated `BinaryFormatter` kullanıyor** — kullanılmıyor ama ileride tercih edilmemeli (§7).
6. Sistem tamamen **Edit Mode / Editor odaklı** tasarlanmış (`EditorApplication.isPlaying` kontrolleri her yerde); runtime'da (build alınmış oyunda) node ekleme/silme/mesh yeniden oluşturma çalışmaz — bu bilinçli bir tasarım kararı gibi görünüyor (level tasarım aracı), sadece build'e dahil edilecekse belirtilmeli.
