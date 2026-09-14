<style>
pre, pre code, code, .hljs {
    direction: ltr !important;
    text-align: left !important;
    unicode-bidi: isolate !important;
}
</style>

<style>
body, p, li, blockquote, h1, h2, h3, h4, h5, h6, table, th, td {
    direction: rtl;
    text-align: right;
    unicode-bidi: plaintext;
}

pre, pre code, code, .hljs {
    direction: ltr !important;
    text-align: left !important;
    unicode-bidi: isolate !important;
}
</style>

# P01-C01-S003 — Nullable Reference Types, Null-Safety & Compiler Flow Analysis

> **Phase 01 — Modern .NET Foundations**  
> **Chapter 01 — Development Environment & Project Bootstrap**  
> **Session 003**

---

## 1. هدف جلسه

هدف این جلسه این است که مفهوم `null` در C# را از یک موضوع صرفاً نحوی به یک **قرارداد طراحی، ابزار تحلیل ایستا و Policy سراسری Build** تبدیل کنیم.

در پایان این جلسه باید بتوانیم:

- تفاوت `string` و `string?` را از دید Contract توضیح دهیم.
- تفاوت Nullable Reference Type با Nullable Value Type را تشخیص دهیم.
- بدانیم `<Nullable>enable</Nullable>` دقیقاً چه چیزی را در Compiler فعال می‌کند.
- تفاوت Annotation Context و Warning Context را توضیح دهیم.
- رفتار `enable`، `disable`، `warnings` و `annotations` را درک کنیم.
- تفاوت Declared Nullability و Flow State را بفهمیم.
- تحلیل Compiler روی `if`، `return`، `throw`، `&&`، `||`، `?.`، `??` و `??=` را پیش‌بینی کنیم.
- بدانیم Null-Forgiving Operator یعنی `!` چه کاری انجام می‌دهد و چه کاری انجام نمی‌دهد.
- Warningهای مهم Nullable مانند `CS8602`، `CS8603`، `CS8604` و `CS8618` را بر اساس Contract تحلیل کنیم.
- Nullable Static Analysis Attributeها را در APIهای پیچیده‌تر به‌درستی به کار ببریم.
- در Domain، Repository، Application و DTOها Nullable Contract مناسب طراحی کنیم.
- Nullable Warningها را در سطح Repository به Build Error تبدیل کنیم.
- Policy نهایی Null-Safety پروژه `CommerceFlow` را در `Directory.Build.props` تثبیت کنیم.

---

## 2. خروجی فنی جلسه

در پایان این Session دو تغییر واقعی در Repository باقی می‌ماند:

1. Policy سراسری Nullable در فایل زیر:

```text
Directory.Build.props
```

2. یک Test آموزشی تمیز در پروژه‌ی Unit Test مربوط به Catalog Domain:

```text
tests/Services/Catalog/CommerceFlow.Catalog.Domain.UnitTests/
└── NullableReferenceTypesTests.cs
```

کدهای عمداً Warningدار فقط برای مرحله‌ی آموزش و Inspection استفاده می‌شوند و در وضعیت نهایی Repository باقی نمی‌مانند.

---

## 3. مسئله‌ی اصلی `null`

Reference Typeها می‌توانند در Runtime مقدار `null` داشته باشند. اگر Reference برابر `null` باشد و بخواهیم Member آن را استفاده کنیم، برنامه می‌تواند با `NullReferenceException` متوقف شود.

مثال:

```csharp
string? customerName = null;

Console.WriteLine(customerName.Length);
```

مسیر مشکل:

```text
customerName
     ↓
   null
     ↓
.Length
     ↓
NullReferenceException
```

Nullable Reference Types برای حذف مفهوم `null` از Runtime طراحی نشده‌اند. هدف آن‌ها این است که Compiler قبل از اجرای برنامه، ناسازگاری میان Contract و نحوه‌ی استفاده از Reference را پیدا کند.

مدل ذهنی:

```text
Developer Intent
      ↓
Nullable Annotations
      ↓
Compiler Flow Analysis
      ↓
Diagnostics
      ↓
Safer Code
```

---

## 4. `string` در برابر `string?`

وقتی Nullable Context فعال باشد:

```csharp
string firstName;
```

یعنی:

```text
firstName is intended to be non-null
```

در مقابل:

```csharp
string? middleName;
```

یعنی:

```text
middleName is allowed to be null
```

بنابراین `?` صرفاً علامتی برای حذف Warning نیست؛ بخشی از Contract کد است.

مثال Domain:

```csharp
public sealed class Customer
{
    public string FirstName { get; }

    public string? MiddleName { get; }

    public string LastName { get; }
}
```

از همین Declaration می‌توانیم بفهمیم:

```text
FirstName  → required
MiddleName → optional
LastName   → required
```

اصل مهم:

> Nullable Annotation باید واقعیت Domain یا API را بیان کند، نه اینکه فقط Compiler را ساکت کند.

---

## 5. تفاوت `string?` و `int?`

این دو Syntax شبیه‌اند اما از نظر Runtime یک مفهوم نیستند.

برای Value Type:

```csharp
int?
```

در واقع shorthand برای این Type است:

```csharp
Nullable<int>
```

اما برای Reference Type:

```csharp
string?
```

یک Runtime Type مستقل به نام `Nullable<string>` ایجاد نمی‌شود.

مدل:

```text
int?
↓
System.Nullable<int>
↓
Distinct runtime representation
```

در مقابل:

```text
string?
↓
Nullable reference annotation
↓
Compile-time nullability contract
```

از دید Runtime، `string` و `string?` همچنان بر پایه‌ی `System.String` هستند. تفاوت اصلی در اطلاعاتی است که Compiler برای Static Analysis استفاده می‌کند.

---

## 6. `<Nullable>` واقعاً چه چیزی را تنظیم می‌کند؟

وقتی می‌نویسیم:

```xml
<Nullable>enable</Nullable>
```

این تنظیم Runtime Protection ایجاد نمی‌کند.

مثلاً کد زیر همچنان می‌تواند در Runtime شکست بخورد:

```csharp
string name = null!;

Console.WriteLine(name.Length);
```

پس:

```text
<Nullable>enable</Nullable>
≠
Runtime null protection
```

این تنظیم در اصل رفتار C# Compiler را در دو حوزه کنترل می‌کند:

```text
Nullable Context
│
├── Annotation Context
│
└── Warning Context
```

---

## 7. Annotation Context

Annotation Context تعیین می‌کند Declarationهایی مثل موارد زیر چه معنایی داشته باشند:

```csharp
string
string?
Customer
Customer?
```

وقتی Annotation Context فعال است:

```text
string  → non-nullable contract
string? → nullable contract
```

یعنی Compiler می‌تواند Intent برنامه‌نویس را درباره‌ی مجاز بودن یا نبودن `null` در Type Contract بفهمد.

وقتی این Context غیرفعال است، Reference Type قدیمی‌تر در وضعیت زیر قرار می‌گیرد:

```text
nullable-oblivious
```

`nullable-oblivious` یعنی Compiler Contract مدرن مشخصی درباره‌ی Nullability آن Reference ندارد.

---

## 8. Warning Context

Warning Context تعیین می‌کند Compiler ناسازگاری‌های Nullability را گزارش کند یا خیر.

مثلاً:

```csharp
string? name = GetName();

Console.WriteLine(name.Length);
```

اگر Nullable Warning Analysis فعال باشد، Compiler مسیر زیر را می‌بیند:

```text
name
↓
maybe-null
↓
.Length
↓
unsafe dereference
↓
CS8602
```

پس دو سؤال مستقل داریم:

```text
Annotations:
What does the declaration mean?

Warnings:
Should the compiler report violations?
```

---

## 9. چهار حالت Nullable Context

چهار مقدار اصلی برای Property زیر وجود دارد:

```xml
<Nullable>...</Nullable>
```

جدول مفهومی:

| مقدار | Annotations | Warnings | کاربرد اصلی |
|---|---:|---:|---|
| `disable` | Off | Off | Legacy/Nullable-oblivious code |
| `annotations` | On | Off | Migration مرحله‌ای |
| `warnings` | Off | On | Migration مرحله‌ای |
| `enable` | On | On | پروژه‌های جدید |

برای `CommerceFlow` انتخاب نهایی:

```xml
<Nullable>enable</Nullable>
```

چون پروژه از ابتدا با Nullable Contract صحیح طراحی می‌شود و نیازی به Migration مرحله‌ای ندارد.

---

## 10. Nullable Reference Types یک قابلیت Compile-Time است

Nullable Reference Types رفتار CLR را عوض نمی‌کند.

Compiler از سه ورودی اصلی استفاده می‌کند:

```text
Annotations
     +
Flow Analysis
     +
Nullable Contract Attributes
     ↓
Diagnostics
```

همچنین هنگام Compile شدن Library، اطلاعات Nullability لازم در Metadata قرار می‌گیرد تا Consumerهای دیگر نیز Contract API را بفهمند.

مثلاً اگر `CommerceFlow.Catalog.Domain` چنین APIای داشته باشد:

```csharp
public string Name { get; }

public string? MiddleName { get; }
```

پروژه‌ای که Assembly آن را Reference می‌کند می‌تواند تفاوت Nullability این دو Member را در زمان Compile درک کند.

---

## 11. Declared Nullability در برابر Flow State

یکی از مهم‌ترین مفاهیم جلسه این است که Declaration Type با State فعلی متغیر یک چیز نیست.

مثلاً:

```csharp
string? name = GetName();
```

Declared Nullability:

```text
string?
```

یعنی Variable اجازه دارد `null` باشد.

State اولیه:

```text
maybe-null
```

اما اگر بنویسیم:

```csharp
if (name is null)
{
    return;
}

Console.WriteLine(name.Length);
```

Declared Type همچنان:

```text
string?
```

است، اما در خط آخر Flow State برابر است با:

```text
not-null
```

زیرا Compiler نتیجه می‌گیرد اگر `name` برابر `null` بود، Execution قبلاً از Method خارج می‌شد.

---

## 12. Null-State Flow Analysis

Compiler مسیرهای اجرایی را تحلیل می‌کند.

مثال:

```csharp
string? name = GetName();

if (name is not null)
{
    Console.WriteLine(name.Length);
}
```

تحلیل:

```text
           name = maybe-null
                  │
                  ▼
          name is not null?
            /           \
         true           false
          │               │
          ▼               ▼
      not-null         maybe-null
          │
          ▼
      name.Length
          ↓
         safe
```

Compiler فقط Type را نگاه نمی‌کند؛ Current State را در هر Branch دنبال می‌کند.

---

## 13. Guard Clause و تغییر Flow State

الگوی بسیار مهم:

```csharp
if (customer is null)
{
    return;
}

customer.PlaceOrder();
```

تحلیل:

```text
customer = maybe-null
        ↓
if null → method exits
        ↓
remaining path
        ↓
customer = not-null
```

همین منطق با `throw` نیز برقرار است:

```csharp
if (customer is null)
{
    throw new InvalidOperationException("Customer was not found.");
}

customer.PlaceOrder();
```

و در کدهای مدرن .NET می‌توان از Guardهای استاندارد هم استفاده کرد:

```csharp
ArgumentNullException.ThrowIfNull(customer);

customer.PlaceOrder();
```

---

## 14. Null Check با Null Handling یکسان نیست

این کد Null Check انجام می‌دهد، ولی مشکل را حل نمی‌کند:

```csharp
if (customer is null)
{
    Console.WriteLine("Customer is null.");
}

customer.PlaceOrder();
```

چون Execution بعد از `if` ادامه پیدا می‌کند.

پس:

```text
checking null
≠
handling null
```

Handling باید Control Flow را تغییر دهد یا یک مقدار جایگزین معتبر ایجاد کند.

---

## 15. تحلیل `&&` و `||`

Compiler Short-Circuit Evaluation را می‌شناسد.

این کد Safe است:

```csharp
if (customer is not null && customer.Name.Length > 0)
{
}
```

چون سمت راست `&&` فقط زمانی ارزیابی می‌شود که سمت چپ `true` باشد.

همچنین:

```csharp
if (customer is null || customer.Name.Length == 0)
{
}
```

در سمت راست `||`، Compiler می‌داند شرط اول `false` شده است، بنابراین `customer` در آن نقطه `not-null` است.

ترتیب شرط مهم است. این کد ایمن نیست:

```csharp
if (customer.Name.Length > 0 && customer is not null)
{
}
```

چون Dereference قبل از Null Check انجام شده است.

---

## 16. Null-Conditional Operator یعنی `?.`

مثال:

```csharp
string? name = customer?.Name;
```

معنی:

```text
customer != null
→ customer.Name

customer == null
→ null
```

زنجیره:

```csharp
string? city = customer?.Address?.City;
```

اگر هر کدام از Referenceهای میانی `null` باشند، نتیجه `null` می‌شود.

نکته:

> `?.` Business Rule را حل نمی‌کند؛ فقط Access را در مقابل `null` ایمن می‌کند.

اگر وجود `Address` یک Invariant واقعی Domain باشد، Nullable کردن آن فقط برای راحتی کدنویسی طراحی خوبی نیست.

---

## 17. Null-Coalescing Operator یعنی `??`

مثال:

```csharp
string displayName = middleName ?? "Unknown";
```

معنی:

```text
middleName != null
→ use middleName

middleName == null
→ use "Unknown"
```

ترکیب با `?.`:

```csharp
string displayName = customer?.Name ?? "Unknown";
```

جریان Type:

```text
customer?.Name
↓
string?
↓
?? "Unknown"
↓
string
```

---

## 18. Null-Coalescing Assignment یعنی `??=`

مثال:

```csharp
string? name = GetName();

name ??= "Unknown";

Console.WriteLine(name.Length);
```

Compiler می‌داند پس از این خط:

```csharp
name ??= "Unknown";
```

در هر دو مسیر مقدار `name` غیر Null است:

```text
name was null
→ assign "Unknown"

name was non-null
→ keep existing value
```

پس Flow State بعد از آن:

```text
not-null
```

است.

---

## 19. Null-Forgiving Operator یعنی `!`

مثال:

```csharp
string? name = GetName();

Console.WriteLine(name!.Length);
```

`!` مقدار Runtime را تغییر نمی‌دهد. فقط Null-State آن Expression را برای Compiler Override می‌کند.

مدل:

```text
name
↓
maybe-null

name!
↓
compiler treats this expression as not-null
```

اگر مقدار واقعی `null` باشد، این کد همچنان می‌تواند در Runtime با `NullReferenceException` شکست بخورد.

بنابراین:

```text
!
│
├── Compiler effect → yes
└── Runtime validation → no
```

اصل پروژه:

> `!` ابزار معمول رفع Nullable Warning نیست. فقط زمانی مجاز است که Guarantee واقعی وجود داشته باشد ولی Compiler نتواند آن را اثبات کند.

---

## 20. `null!` و Initialization مصنوعی

این الگو زیاد دیده می‌شود:

```csharp
public string FirstName { get; set; } = null!;
```

این کد می‌تواند Warning مربوط به Initialization را حذف کند، اما Property در Runtime همچنان در ابتدا `null` است.

پس:

```text
null!
↓
warning suppression
≠
valid object initialization
```

در Domain معمولاً Constructor-based Initialization انتخاب بهتری است.

برای DTOها نیز در بسیاری از موارد `required` Contract واضح‌تری می‌دهد.

---

## 21. `required` چه مسئله‌ای را حل می‌کند؟

برای Data Carrier Type:

```csharp
public sealed class CreateCustomerRequest
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? MiddleName { get; init; }
}
```

Caller باید Memberهای `required` را مقداردهی کند:

```csharp
var request = new CreateCustomerRequest
{
    FirstName = "Kamran",
    LastName = "Example"
};
```

اما `required` با Business Validation یکسان نیست.

مثلاً مقدار زیر همچنان non-null است ولی احتمالاً نامعتبر است:

```csharp
FirstName = "   ";
```

پس:

```text
required
↓
initialization contract
```

در مقابل:

```text
validation
↓
value correctness contract
```

---

## 22. Warning مهم `CS8602`

نمونه:

```csharp
public static int GetLength(string? value)
{
    return value.Length;
}
```

Diagnostic:

```text
CS8602
Dereference of a possibly null reference.
```

Contract:

```text
value → maybe-null
.Length → dereference
```

راه‌حل باید براساس Meaning واقعی `null` انتخاب شود.

اگر `null` معتبر است و Length باید صفر شود:

```csharp
public static int GetLength(string? value)
{
    return value?.Length ?? 0;
}
```

اگر `null` اصلاً ورودی معتبر نیست:

```csharp
public static int GetLength(string value)
{
    ArgumentNullException.ThrowIfNull(value);

    return value.Length;
}
```

---

## 23. Warning مهم `CS8603`

مثال:

```csharp
public static string GetMiddleName(Customer customer)
{
    return customer.MiddleName;
}
```

اگر `MiddleName` از نوع زیر باشد:

```csharp
string?
```

Return Contract Method می‌گوید:

```text
string → caller may assume non-null
```

اما Implementation ممکن است `null` برگرداند.

Diagnostic:

```text
CS8603
Possible null reference return.
```

اگر نبودن مقدار معتبر است:

```csharp
public static string? GetMiddleName(Customer customer)
{
    return customer.MiddleName;
}
```

اگر Method باید همیشه مقدار بدهد:

```csharp
public static string GetDisplayName(Customer customer)
{
    return customer.MiddleName ?? "Unknown";
}
```

---

## 24. Warning مهم `CS8604`

مثال:

```csharp
static void SendEmail(string email)
{
}

string? email = GetEmail();

SendEmail(email);
```

Contract mismatch:

```text
argument → maybe-null
parameter → requires non-null
```

Diagnostic:

```text
CS8604
Possible null reference argument for parameter.
```

راه درست می‌تواند Guard باشد:

```csharp
if (email is null)
{
    return;
}

SendEmail(email);
```

یا اگر API واقعاً `null` را قبول می‌کند، Parameter Contract باید صادقانه تغییر کند.

---

## 25. Warning مهم `CS8618`

مثال:

```csharp
public sealed class Customer
{
    public string FirstName { get; set; }
}
```

`FirstName` Non-Nullable است، اما Constructor ضمنی هیچ مقدار معتبری برای آن ایجاد نکرده است.

Diagnostic:

```text
CS8618
Non-nullable member must contain a non-null value when exiting constructor.
```

برای Domain Object، راه‌حل مناسب معمولاً Establish کردن Invariant در Constructor است:

```csharp
public sealed class Customer
{
    public Customer(string firstName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);

        FirstName = firstName;
    }

    public string FirstName { get; }
}
```

این بهتر از این است:

```csharp
public string FirstName { get; set; } = null!;
```

چون Object از لحظه‌ی Construction در State معتبر قرار می‌گیرد.

---

## 26. Nullable Warningها را به‌عنوان Contract Violation بخوانیم

مدل ساده:

```text
CS8600 / CS8601
maybe-null value
→ non-null storage

CS8602
maybe-null reference
→ dereference

CS8603
maybe-null result
→ non-null return contract

CS8604
maybe-null argument
→ non-null parameter contract

CS8618
non-null member
→ initialization invariant not established

CS8625
null literal
→ non-null destination
```

اصل مهم:

> سؤال اصلی «چطور Warning را حذف کنم؟» نیست؛ سؤال اصلی «کدام Contract اشتباه یا ناقص است؟» است.

---

## 27. Nullable Static Analysis Attributes

گاهی `T` و `T?` برای توصیف کامل رفتار یک API کافی نیستند.

Attributeهای اصلی در Namespace زیر قرار دارند:

```csharp
System.Diagnostics.CodeAnalysis
```

دسته‌بندی:

```text
Nullable Analysis Attributes
│
├── Preconditions
│   ├── AllowNull
│   └── DisallowNull
│
├── Postconditions
│   ├── MaybeNull
│   └── NotNull
│
├── Conditional Postconditions
│   ├── MaybeNullWhen
│   ├── NotNullWhen
│   └── NotNullIfNotNull
│
└── Member State
    ├── MemberNotNull
    └── MemberNotNullWhen
```

این Attributeها Runtime Validation اضافه نمی‌کنند. آن‌ها Contract API را برای Static Analyzer توضیح می‌دهند.

---

## 28. مثال `[NotNullWhen]`

Helper:

```csharp
using System.Diagnostics.CodeAnalysis;

public static bool HasText(
    [NotNullWhen(true)] string? value)
{
    return !string.IsNullOrWhiteSpace(value);
}
```

Caller:

```csharp
string? email = GetEmail();

if (HasText(email))
{
    Console.WriteLine(email.Length);
}
```

Contract Attribute می‌گوید:

```text
HasText(...) == true
↓
value is not null
```

پس Compiler می‌تواند داخل Branch، State آرگومان را `not-null` در نظر بگیرد.

---

## 29. مثال `[NotNull]`

```csharp
using System.Diagnostics.CodeAnalysis;

public static void EnsureNotNull(
    [NotNull] object? value)
{
    if (value is null)
    {
        throw new ArgumentNullException(nameof(value));
    }
}
```

Caller:

```csharp
Customer? customer = FindCustomer();

EnsureNotNull(customer);

customer.PlaceOrder();
```

بعد از بازگشت عادی Method، Compiler می‌تواند `customer` را `not-null` بداند.

---

## 30. مثال `[MemberNotNull]`

وقتی Helper Method مسئول Initialize کردن Memberهای یک Object است:

```csharp
using System.Diagnostics.CodeAnalysis;

public sealed class CustomerNameHolder
{
    private string _name;

    public CustomerNameHolder(string name)
    {
        Initialize(name);
    }

    [MemberNotNull(nameof(_name))]
    private void Initialize(string name)
    {
        _name = name;
    }
}
```

Attribute به Compiler اعلام می‌کند بعد از بازگشت عادی `Initialize`، Member مشخص‌شده `null` نیست.

اصل مهم:

> Nullable Attribute باید واقعیت Implementation را توصیف کند. Attribute نادرست می‌تواند Compiler را بر اساس Contract دروغین به نتیجه‌ی اشتباه برساند.

---

## 31. Nullability در Domain

در Domain، Nullable باید Business State واقعی را بیان کند.

مثال مناسب:

```csharp
public sealed class Customer
{
    public Customer(
        Guid id,
        string firstName,
        string lastName,
        string? middleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        Id = id;
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }

    public Guid Id { get; }

    public string FirstName { get; }

    public string? MiddleName { get; }

    public string LastName { get; }
}
```

در این مدل:

```text
MiddleName == null
```

یک State معتبر Domain است.

اما اگر `FirstName` الزامی باشد، Nullable کردن آن برای حذف Warning طراحی نادرستی است.

---

## 32. Nullability در Value Object

اگر `Email` یک Value Object معتبر باشد، بهتر است Invalid State داخل خود Value Object قرار نگیرد.

مدل بهتر:

```csharp
public Email? Email { get; }
```

اگر Email اختیاری است.

بهتر از این طراحی است:

```csharp
public Email Email { get; }
```

که داخل `Email` داشته باشیم:

```csharp
public string? Value { get; }
```

اصل:

```text
Optional object
→ T?

Existing value object
→ internally valid
```

---

## 33. Nullability در Repository

Repository ممکن است Entity را پیدا نکند.

Contract طبیعی:

```csharp
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken);
}
```

معنی:

```text
record exists
→ Customer

record does not exist
→ null
```

اگر API دیگری Guarantee می‌کند Entity وجود دارد یا Failure Path ایجاد می‌کند، می‌تواند Non-Nullable برگرداند:

```csharp
Task<Customer> GetRequiredByIdAsync(...);
```

ولی Implementation باید این Guarantee را واقعاً برقرار کند.

---

## 34. Nullability در Application Layer

فرض کنیم Repository برمی‌گرداند:

```csharp
Task<Customer?>
```

Application باید absence را تفسیر کند:

```csharp
var customer =
    await customerRepository.GetByIdAsync(
        request.CustomerId,
        cancellationToken);

if (customer is null)
{
    return Result.Failure(
        CustomerErrors.NotFound(request.CustomerId));
}

customer.PlaceOrder();
```

جریان:

```text
Persistence absence
        ↓
Repository null
        ↓
Application interpretation
        ↓
NotFound result
```

این بسیار بهتر از عبور بی‌معنای `null` بین Layerهاست.

---

## 35. Nullability در Request و Response DTO

Request نمونه:

```csharp
public sealed class CreateCustomerRequest
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? MiddleName { get; init; }
}
```

Response نمونه:

```csharp
public sealed record CustomerResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName);
```

Nullability در Public Contract باید همان چیزی را بگوید که Consumer واقعاً باید انتظار داشته باشد.

اصل:

> برای «احتیاط» همه‌ی Propertyها را Nullable نکنیم. Contract ضعیف‌تر از واقعیت نیز یک Contract بد است.

---

## 36. Collectionها و `null`

اگر Customer هیچ Orderی ندارد، معمولاً بهتر است:

```csharp
IReadOnlyCollection<Order> Orders
```

یک Collection خالی برگرداند، نه:

```csharp
IReadOnlyCollection<Order>? Orders
```

مدل پیش‌فرض:

```text
zero items
→ empty collection
```

نه:

```text
zero items
→ null collection
```

مگر اینکه `null` واقعاً State جداگانه‌ای مانند «اطلاعات Load نشده است» را بیان کند و این طراحی آگاهانه باشد.

---

## 37. Optional با Unknown یکسان نیست

مثلاً:

```csharp
DateTime? DeliveredAt
```

می‌تواند به‌خوبی این معنا را داشته باشد:

```text
null     → not delivered yet
non-null → delivered at this time
```

اما اگر سه State واقعی داشته باشیم:

```text
Unknown
Pending
Delivered
```

ممکن است یک Type صریح مانند Enum مدل بهتری باشد.

اصل:

> فقط چون `?` می‌تواند چند State را به شکلی فشرده نمایش دهد، الزاماً بهترین Domain Model نیست.

---

## 38. Policy مرکزی Nullable در `CommerceFlow`

Policy نهایی این جلسه در Root Repository داخل فایل زیر قرار می‌گیرد:

```text
Directory.Build.props
```

نسخه‌ی فعلی:

```xml
<Project>

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <WarningsAsErrors>$(WarningsAsErrors);nullable</WarningsAsErrors>
  </PropertyGroup>

</Project>
```

دو خط اصلی Session 3:

```xml
<Nullable>enable</Nullable>
<WarningsAsErrors>$(WarningsAsErrors);nullable</WarningsAsErrors>
```

---

## 39. تفاوت `WarningsAsErrors` و `TreatWarningsAsErrors`

این دو Property یک مفهوم نیستند.

`TreatWarningsAsErrors` یک Boolean است:

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

یا:

```xml
<TreatWarningsAsErrors>false</TreatWarningsAsErrors>
```

و Policy گسترده‌ای روی Warningهای Compiler ایجاد می‌کند.

اما `WarningsAsErrors` یک List Property است و Warningها یا گروه‌های انتخاب‌شده را به Error ارتقا می‌دهد:

```xml
<WarningsAsErrors>$(WarningsAsErrors);nullable</WarningsAsErrors>
```

مدل:

```text
TreatWarningsAsErrors
↓
Boolean global compiler policy
```

در مقابل:

```text
WarningsAsErrors
↓
selected warning IDs / categories
```

در این Session فقط Nullable Warningها را Build-Breaking می‌کنیم و فعلاً همه‌ی Warningهای Repository را با `TreatWarningsAsErrors=true` به Error تبدیل نمی‌کنیم.

---

## 40. چرا `$(WarningsAsErrors)` حفظ می‌شود؟

این مقدار:

```xml
<WarningsAsErrors>$(WarningsAsErrors);nullable</WarningsAsErrors>
```

به‌جای Replace کردن List قبلی، مقدار موجود را Extend می‌کند.

مدل:

```text
existing WarningsAsErrors
        +
nullable
```

این الگوی استاندارد MSBuild برای افزودن مقدار به یک Property لیستی است.

---

## 41. معنی معماری Policy نهایی

این دو خط را فقط به‌عنوان تنظیم Compiler نخوانیم:

```xml
<Nullable>enable</Nullable>
<WarningsAsErrors>$(WarningsAsErrors);nullable</WarningsAsErrors>
```

معنای واقعی Repository Policy:

```text
All projects participate in nullable analysis.
        ↓
APIs must explicitly communicate absence.
        ↓
Roslyn tracks null-state through control flow.
        ↓
Nullable contract violations are build-breaking.
```

از این جلسه به بعد Null-Safety یک Preference محلی در `.csproj` نیست؛ یک Policy سراسری Repository است.

---

## 42. `Directory.Build.props` و Scope تنظیمات

اگر ساختار Repository چنین باشد:

```text
commerceflow-dotnet10-microservices/
│
├── Directory.Build.props
│
├── src/
│   └── Services/
│       └── Catalog/
│           └── CommerceFlow.Catalog.Domain/
│
└── tests/
    └── Services/
        └── Catalog/
            └── CommerceFlow.Catalog.Domain.UnitTests/
```

Projectهای زیر این Root می‌توانند تنظیمات `Directory.Build.props` را دریافت کنند.

نکته‌ی MSBuild:

`Directory.Build.props` زود Import می‌شود، بنابراین یک `.csproj` می‌تواند Property را بعداً Override کند. در این مرحله Policy تیمی ما این است که Nullability Configuration بدون دلیل معماری صریح در Projectهای فرزند Override نشود.

اگر در آینده Enforcement غیرقابل‌تخطی لازم باشد، می‌توان از ابزارهای Build Governance و Importهای دیرتر مانند `Directory.Build.targets` استفاده کرد.

---

## 43. آزمایش آموزشی `CS8602`

برای اثبات Policy، می‌توانیم موقتاً کد زیر را در Test Project ایجاد کنیم:

```csharp
private static int BreakNullablePolicy(string? value)
{
    return value.Length;
}
```

سپس:

```powershell
dotnet build
```

انتظار آموزشی:

```text
error CS8602: Dereference of a possibly null reference.
```

کلمه‌ی `error` مهم است؛ یعنی Nullable Diagnostic به‌دلیل Policy زیر Build-Breaking شده است:

```xml
<WarningsAsErrors>$(WarningsAsErrors);nullable</WarningsAsErrors>
```

بعد از Verification، کد عمداً ناسالم حذف می‌شود.

---

## 44. Test نهایی Session

فایل پیشنهادی:

```text
tests/Services/Catalog/CommerceFlow.Catalog.Domain.UnitTests/
NullableReferenceTypesTests.cs
```

محتوای تمیز:

```csharp
namespace CommerceFlow.Catalog.Domain.UnitTests;

public sealed class NullableReferenceTypesTests
{
    [Fact]
    public void GetSafeLength_WhenValueIsNull_ShouldReturnZero()
    {
        var result = GetSafeLength(null);

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetSafeLength_WhenValueHasContent_ShouldReturnLength()
    {
        var result = GetSafeLength("CommerceFlow");

        Assert.Equal(12, result);
    }

    [Fact]
    public void GetLengthWithFallback_WhenValueIsNull_ShouldReturnZero()
    {
        var result = GetLengthWithFallback(null);

        Assert.Equal(0, result);
    }

    private static int GetSafeLength(string? value)
    {
        if (value is null)
        {
            return 0;
        }

        return value.Length;
    }

    private static int GetLengthWithFallback(string? value)
    {
        return value?.Length ?? 0;
    }
}
```

این Test دو راه صحیح Handling را نشان می‌دهد:

```text
Explicit control-flow guard
```

و:

```text
Null-conditional access + fallback
```

بدون استفاده از `!`.

---

## 45. Build Verification نهایی

از Root Repository:

```powershell
cd <repository-root>
```

ابتدا Clean:

```powershell
dotnet clean
```

سپس Build:

```powershell
dotnet build
```

و بعد Test:

```powershell
dotnet test
```

هدف نهایی:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

و همه‌ی Testها باید Pass شوند.

---

## 46. مشاهده‌ی Propertyهای Evaluate‌شده توسط MSBuild

برای اینکه مقدار نهایی Policy را مستقیماً از MSBuild ببینیم، می‌توانیم Project تست را پیدا کنیم:

```powershell
$p = (Get-ChildItem -Recurse -Filter CommerceFlow.Catalog.Domain.UnitTests.csproj).FullName
```

سپس:

```powershell
dotnet msbuild $p -getProperty:Nullable,WarningsAsErrors
```

انتظار مفهومی:

```text
Nullable = enable
WarningsAsErrors = ...;nullable
```

این روش نشان می‌دهد Property نهایی بعد از MSBuild Evaluation چیست، نه اینکه صرفاً از روی فایل Configuration حدس بزنیم.

---

## 47. Audit اختیاری Policy در Repository

برای پیدا کردن محل‌هایی که Warning Policy تعریف شده است:

```powershell
Get-ChildItem -Recurse -File -Include *.props,*.targets,*.csproj |
    Select-String -Pattern 'TreatWarningsAsErrors|WarningsAsErrors|Nullable'
```

این Command برای Audit Configuration مفید است و کمک می‌کند Overrideهای ناخواسته را پیدا کنیم.

---

## 48. تفاوت Local Build Policy با Command-Line Override

Command زیر نیز می‌تواند Warningها را به Error تبدیل کند:

```powershell
dotnet build -warnaserror
```

اما این Session عمداً Policy Nullable را داخل Repository نگه می‌دارد:

```xml
<WarningsAsErrors>$(WarningsAsErrors);nullable</WarningsAsErrors>
```

مزیت:

```text
Visual Studio
+
dotnet build
+
CI
↓
shared repository policy
```

هدف این است که Developer محلی و CI تا حد ممکن رفتار متفاوتی درباره‌ی Nullability نداشته باشند.

---

## 49. قوانین Null-Safety پروژه `CommerceFlow`

از این Session به بعد قواعد زیر اعمال می‌شوند:

1. Nullable Reference Types در کل Repository فعال است.
2. Nullable Warningها Build-Breaking هستند.
3. `T?` فقط زمانی استفاده می‌شود که absence واقعاً State معتبر باشد.
4. `!` فقط با Guarantee واقعی و قابل توضیح مجاز است.
5. `#nullable disable` در کد عادی پروژه استفاده نمی‌شود.
6. Global Suppression خانواده‌ی Nullable با `NoWarn` مجاز نیست.
7. Domain Entity باید تا حد ممکن بعد از Construction در State معتبر باشد.
8. Optional Collection به‌صورت پیش‌فرض با Empty Collection مدل می‌شود، نه `null`.
9. Repository absence را با Contract صادقانه مانند `Customer?` بیان می‌کند.
10. Application Layer باید absence را به Meaning مناسب Use Case تبدیل کند.
11. Nullable Annotation جای Validation را نمی‌گیرد.
12. Nullable Attributes فقط زمانی استفاده می‌شوند که `T` و `T?` برای بیان Contract کافی نباشند.

---

## 50. Anti-Patternهای ممنوع

### Nullable کردن همه‌چیز برای حذف Warning

```csharp
public string? FirstName { get; set; }
```

وقتی `FirstName` واقعاً Required است.

### استفاده‌ی بی‌دلیل از `!`

```csharp
customer!.Name
```

فقط برای Clean شدن Build.

### Initialization مصنوعی Domain Member

```csharp
public string FirstName { get; set; } = null!;
```

وقتی Constructor می‌تواند Invariant را برقرار کند.

### خاموش کردن Nullable در Source File

```csharp
#nullable disable
```

بدون دلیل فنی مشخص.

### پنهان کردن Nullable Warningها در Repository

```xml
<NoWarn>$(NoWarn);nullable</NoWarn>
```

این رویکرد با Policy `CommerceFlow` سازگار نیست.

---

## 51. Decision Tree برای رفع Nullable Warning

وقتی Nullable Warning می‌بینیم:

```text
آیا null واقعاً State معتبر است؟
        │
        ├── Yes
        │    ↓
        │   T? + explicit handling
        │
        └── No
             ↓
آیا Type/API Contract اشتباه است؟
             │
             ├── Yes → fix the contract
             │
             └── No
                  ↓
آیا Runtime validation لازم است؟
                  │
                  ├── Yes → guard / throw
                  │
                  └── No
                       ↓
آیا Guarantee واقعی وجود دارد که Compiler نمی‌فهمد؟
                       │
                       ├── Yes → attribute or carefully justified !
                       └── No → redesign
```

این Decision Tree از حفظ کردن Warning Codeها مهم‌تر است.

---

## 52. مدل ذهنی نهایی Session

کل Session را می‌توان در این Pipeline خلاصه کرد:

```text
Business / API Intent
        ↓
T or T?
        ↓
Nullable Annotations
        ↓
Declared Nullability
        ↓
Control Flow
        ↓
Compiler Null-State Analysis
        ↓
not-null / maybe-null
        ↓
Nullable Diagnostics
        ↓
WarningsAsErrors = nullable
        ↓
Build Gate
```

و برای داده‌هایی که از Boundaryهای غیرقابل‌اعتماد وارد می‌شوند:

```text
External Input
      ↓
Runtime Validation / Guard
      ↓
Domain Invariant
```

پس:

```text
Compile-time safety
+
Runtime validation
```

مکمل یکدیگر هستند.

---

## 53. Commands Reference

دستورات اصلی این Session:

```powershell
# Navigate to repository
cd <repository-root>

# Baseline / final verification
dotnet clean
dotnet build
dotnet test

# Optional: force all warnings as errors for one command-line build
dotnet build -warnaserror

# Locate the Unit Test project
$p = (Get-ChildItem -Recurse -Filter CommerceFlow.Catalog.Domain.UnitTests.csproj).FullName

# Inspect effective MSBuild nullable properties
dotnet msbuild $p -getProperty:Nullable,WarningsAsErrors

# Audit nullable/warning configuration across project files
Get-ChildItem -Recurse -File -Include *.props,*.targets,*.csproj |
    Select-String -Pattern 'TreatWarningsAsErrors|WarningsAsErrors|Nullable'
```

---

## 54. Repository Changes Summary

تغییرات ماندگار Session:

```text
Directory.Build.props
```

شامل:

```xml
<Nullable>enable</Nullable>
<WarningsAsErrors>$(WarningsAsErrors);nullable</WarningsAsErrors>
```

و Test آموزشی:

```text
tests/Services/Catalog/
└── CommerceFlow.Catalog.Domain.UnitTests/
    └── NullableReferenceTypesTests.cs
```

فایل‌های Demo عمداً Unsafe در پایان Session حذف می‌شوند.

---

## 55. Definition of Done

- [x] تفاوت `T` و `T?` در Reference Typeها درک شده است.
- [x] تفاوت Nullable Reference Type و Nullable Value Type مشخص است.
- [x] Annotation Context و Warning Context بررسی شده‌اند.
- [x] چهار Nullable Mode بررسی شده‌اند.
- [x] Null-State Flow Analysis بررسی شده است.
- [x] `?.`، `??` و `??=` بررسی شده‌اند.
- [x] Null-Forgiving Operator بررسی شده است.
- [x] Warningهای اصلی Nullable تحلیل شده‌اند.
- [x] Nullable Static Analysis Attributeها بررسی شده‌اند.
- [x] قواعد Nullability برای Domain و API تعریف شده‌اند.
- [x] Nullable در سطح Repository فعال شده است.
- [x] Nullable Warningها به Build Error تبدیل شده‌اند.
- [x] Test نهایی Session تعریف شده است.
- [ ] Final `dotnet clean` اجرا و تأیید شود.
- [ ] Final `dotnet build` با `0 Warning(s)` و `0 Error(s)` تأیید شود.
- [ ] Final `dotnet test` با Testهای Pass تأیید شود.
- [ ] Session booklet Commit شود.
- [ ] Commit به `origin/Develop` Push شود.
- [ ] Session Tag ایجاد و Push شود.

---

## 56. Git Closeout Plan

بعد از قرار دادن این جزوه در مسیر زیر:

```text
docs/sessions/P01/
P01-C01-S003 — Nullable Reference Types, Null-Safety & Compiler Flow Analysis.md
```

ابتدا وضعیت Repository بررسی می‌شود:

```powershell
git status
```

سپس تغییرات Stage می‌شوند:

```powershell
git add .
```

Audit قبل از Commit:

```powershell
git diff --cached --name-status
```

Commit پیشنهادی:

```powershell
git commit -m "feat(P01-C01-S003): enforce nullable reference type safety"
```

Push:

```powershell
git push
```

Tag نهایی Session:

```powershell
git tag -a p01-c01-s003 -m "P01-C01-S003 — Nullable Reference Types, Null-Safety & Compiler Flow Analysis"
```

Push Tag:

```powershell
git push origin p01-c01-s003
```

Verification:

```powershell
git status
git log --oneline --decorate -5
git tag -n
```

Tag باید بعد از Commit شدن جزوه و تمام Artifactهای Session ساخته شود تا Snapshot نهایی Session را نشان دهد.

---

## 57. Session Completion Rule

`P01-C01-S003` زمانی کامل است که:

```text
Learn
  ↓
Implement
  ↓
Verify
  ↓
Document
  ↓
Commit
  ↓
Push
  ↓
Tag
```

به‌طور کامل انجام شده باشد.

در پایان، Repository باید هم از نظر Code و هم از نظر Documentation یک Checkpoint قابل بازیابی برای Session سوم داشته باشد.

---

## 58. منابع رسمی پیشنهادی

- Microsoft Learn — Nullable reference types  
  `https://learn.microsoft.com/dotnet/csharp/nullable-references`

- Microsoft Learn — Nullable reference type warnings  
  `https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/nullable-warnings`

- Microsoft Learn — Nullable static analysis attributes  
  `https://learn.microsoft.com/dotnet/csharp/language-reference/attributes/nullable-analysis`

- Microsoft Learn — C# compiler warning options  
  `https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-options/errors-warnings`

- Microsoft Learn — Customize the build by folder  
  `https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory`

- Microsoft Learn — How MSBuild builds projects  
  `https://learn.microsoft.com/visualstudio/msbuild/build-process-overview`

---

## 59. جمع‌بندی

در این Session، Nullable Reference Types به‌عنوان یک Syntax منفرد بررسی نشد؛ بلکه به‌عنوان بخشی از Design Contract پروژه تعریف شد.

مهم‌ترین نتیجه:

```text
null is not a default escape hatch.
null must have a meaning.
```

در `CommerceFlow` از اینجا به بعد هر Nullable Contract باید قابل توضیح باشد، Compiler باید بتواند Flow آن را تحلیل کند، و نقض Nullable Contract نباید از Build عبور کند.

---

**Next:** `P01-C01-S004`
