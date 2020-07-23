Language Specification
----------------------

FARR stands for Find And Replace Robot
and is a tool that is designed to do a mass structural find and replace;
however, the language and the engine is designed to be more general than that,
meaning, it's possible to use the language independently from the tool to fit different use cases.

## FARR File

A FARR file ends with the extension `.farr` and contains the input
to the FARR engine.

A simple FARR file looks like the following:
```
[SectionName]
ItemA > ItemB
```

FARR treats the source and the target as objects as opposed to a plain text
where each object has a well defined type such as `String`, `Regex`, `Text`
in the example above `ItemA` and `ItemB` are plain text hence their type is `Text`.

* When an empty file is passed to the FARR engine it should prints a friendly message that tells the user the file is empty.

* When an error occurs during a transformation the transaction is cancelled and rolled back.

## Section

A section is a fixed set of items that are processed as a unit by the engine.

* Can be labeled. A label accepts only letters and intermediate spaces as defined by Unicode `Lu`, `Ll`, `Lt`, `Lm`, `Lo`, `U+0020`.

* Is a fixed set therefore during processing items cannot be modified, added or removed.

## Item

An item holds the content that is expressed by a series of Unicode characters
and has a well defined type in the language depending on the shape of the item.

* Can be listed as a single item.

* Can be listed as a key-value pair using `:`.

* Can be listed as a transformation using `>` that is a syntactic sugar to the key-value pair with the `@Transform` annotation that applies to the item.

## Type

A type can be specified to inform the FARR engine how it should treat the content.

* Can be specified to the right of an `Item` or a `Section` using `::`
  and start with a Unicode character in the following categories `Lu`, `Ll`, `Lt`, `Lm`, `Lo`
  that can be followed by `Nd` and `Nl`.

* Can be extended via plugins.

### Example:

```
"(class\s+)"@source > "$1"@target :: Regex(Foo > FooBar)
```

### Pseudo Constructor

A pseudo constructor is a construct that allows values to be passed to the FARR engine.

* Can have zero or more arguments separated by `,`.

* More details can be [found here](Language%20Specification.md#farr-engine-net-core-types).

### Built-in Types

All of the built-in types can be inferred from the context so there is no need to specify the type explicitly;
however, sometimes we may want to pass valuable information to the FARR engine,
in this case, we can be explicit and pass the information using the pseudo constructor of the type, e.g., `Regex(Foo > FooBar)`.

* `Text`

  * Is a series of arbitrary Unicode characters.

  * Can contain only intermediate spaces.

  * Can be concatenated with any other `RawString`, `String` or `Text` on the same side of the line that produces a single .NET `String` instance.

  * Cannot have leading spaces as they are treated as indentations and as such are considered as `Subitem`s.

  * Cannot have trailing spaces as they might get added for alignment and as such are considered insignificant whitespaces.

  * Cannot be passed as an argument.

* `String`

  * Is a series of Unicode characters quoted by `"`.
    * The exact syntax is specified by the [Syntax Definition](Syntax%20Definition.md#String) file.

  * Can be concatenated with any other `RawString`, `String` or `Text` on the same side of the line that produces a single .NET `String` instance.

* `RawString`

  * Has to start with `@` immediately followed by `'`
    and an optional delimiter that is a series of Unicode characters in the following categories `Lu`, `Ll`, `Lt`, `Lm`, `Lo`
    continuing with `"` and then the content interpreted exactly as they appear in the source.
    The terminator of the string literal is a `"` immediately followed by the optional delimiter and `'`.

  * Can be concatenated with any other `RawString`, `String` or `Text` on the same side of the line that produces a single .NET `String` instance.

  ### Example:

  ```
  @'foo"<foo bar=" > "></foo>"foo'
  ```

  The difference between a `String` and a `RawString` is that the former should be used for simple cases where content of the string doesn't need to be escaped
  whereas with the latter it's possible to have an optional custom delimiter and avoid escaping at all so it's more flexible but slightly more verbose.

  You can think about `Text`, `String` and `RawString` as a spectrum that you can dial up/down based on the need.

* `Number`

  * Is a series of the following Unicode characters:

    | Unicode         | Literal |
    | --------------- | :-----: |
    | U+0030 - U+0039 |  0 - 9  |
    | U+0020          |  Space  |
    | U+2009          |  Space  |
    | U+202F          |  Space  |
    | U+002E          |    .    |
    | U+00B7          |    ·    |
    | U+02D9          |    ˙    |
    | U+0027          |    '    |
    | U+002C          |    ,    |
    | U+066B          |    ٫    |
    | U+066C          |    ٬    |
    | U+2396          |    ⎖    |

    * The exact syntax is specified by the [Syntax Definition](Syntax%20Definition.md#Number) file.

  * Can have a fixed size by passing the following constants to the pseudo constructor:
    
    | Constants | .NET Type |
    | --------- | :-------: |
    | i8        |   SByte   |
    | u8        |   Byte    |
    | i16       |   Int16   |
    | u16       |  UInt16   |
    | i32       |   Int32   |
    | u32       |  UInt32   |
    | i64       |   Int64   |
    | u64       |  UInt64   |
    | f32       |  Single   |
    | f64       |  Double   |

* `Regex`

  * Supports the same syntax and semantics as defined by .NET Regular Expressions.

  * Defines a pseudo constructor that takes multiple `Section`s or `Item`s as arguments that forms a union.

  * Defines two contextual keywords `@source` and `@target` that relate to the input that was passed through the pseudo constructor.
    * When an `Item` does not have a target and `@target` is specified it is an error.

## Annotation

An annotation has to start with `@`
followed by Unicode characters in the following categories `Lu`, `Ll`, `Lt`, `Lm`, `Lo` that can be followed by `Nd` and `Nl`.

* Can be applied to a `Section` or an `Item` to pass information to the engine.

* Can have zero or more arguments separated by `,`.

* Can be extended via plugins.

The difference between a `Type` and an `Annotation` is that
the former defines the way the FARR engine interprets the content
and the latter provides the behaviour or action that applies to the content,
in fact, conceptually you can think about `Annotation`s as a special case of `Type`s with the benefit of executing an action.

### Example:

```
"(class\s+)"@source : "$1"@target :: Regex(Foo > FooBar), @Transform
```

### Built-in Annotations

* `@Add`

* `@Include`

* `@Use`

* `@Transform`

## Keywords

## Comments

A single-line comment starts with `#`.

A multi-line comment starts with `<#` and ends with `#>`.

Comments are ignored by the syntax.

## FARR Engine: .NET Core Types

### Classes

Users can create custom `Type`s and `Annotation`s by deriving from the following .NET classes:

* `FarrType`

* `FarrAnnotation`

### Interfaces

* `IHasPseudoConstructor`

  * When a .NET class that derives from `FarrType` is created and then loaded by the engine, 
    it checks for the `IHasPseudoConstructor` interface and then use the `Invoke` method to get the arguments that were passed to the type.