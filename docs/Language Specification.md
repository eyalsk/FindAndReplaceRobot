Language Specification
----------------------

FARR stands for Find And Replace Robot
and is a tool that is designed to do a mass structural find and replace;
however, the language and the FARR engine (the "engine") is designed to be more general than that,
meaning, it's possible to use the language independently from the tool to fit different use cases.

## FARR File

A FARR file ends with the extension `.farr` and contains the input
to the engine.

A simple FARR file looks like the following:
```
[SectionName]
ItemA > ItemB
```

FARR treats the source and the target as objects as opposed to a plain text
where each object has a well defined type such as `String`, `Regex`, `Text`
in the example above `ItemA` and `ItemB` are plain text hence their type is `Text`.

* When an empty file is passed to the engine it should prints a friendly message that tells the user the file is empty.

* When an error occurs during a transformation the transaction is cancelled and rolled back.

## Items

An `Item` holds the content which is a piece of text or a series of Unicode characters that has well defined form.

* Has a well defined [Type](Language%20Specification.md#types)

* Has a left-hand side (`LHS`) and an optional delimiter (`:`) for the right-hand side (`RHS`).
  * Can be listed as a single `Item`.
  * Can be listed as a key-value pair.
  * Can be listed as a transformation using `>` instead of `:`.

* Can have children (`Subitem`s) by adding [Indentations](Language%20Specification.md#Indentation) to `Item`s. `Subitem`s of the same indentation level are considered part of the same `Section` which forms a `Nested Section`.

## Sections

A `Section` is a fixed set of `Item`s that are separated by [EOL](Language%20Specification.md#end-of-line) and finally ends with an additional [EOL](Language%20Specification.md#end-of-line).

* Can be [Labeled](Language%20Specification.md#label).

* Can be anonymous. A `Section` that starts without a [Label](Language%20Specification.md#label) forms an `Anonymous Section`.

## Types

A `Type` indicates how the engine should treat a `Section` or an `Item`.

* Can be specified to the right of an `Item` or a `Section` by `::` followed by an [Identifier](Language%20Specification.md#identifier).
  * When applied to a `Section` the `Type` does not apply to the `Section` itself but to the `Item`s of the `Section`.

* Can have a [Pseudo Constructor](Language%20Specification.md#pseudo-constructors).

* Can be extended via plugins.

### Example:

```
"(class\s+)"@source > "$1"@target :: Regex(Foo > FooBar)
```

### Built-in Types

All of the built-in types can be inferred from the context unless noted otherwise so there is no need to specify the `Type` explicitly.

* `Text`

  * Is a series of arbitrary Unicode characters.

  * Can contain only intermediate spaces.

  * Can be concatenated with any other `RawString`, `String` or `Text` on the same side of the line that produces a single .NET `String` instance.

  * Cannot have leading spaces as they are treated as [Indentations](Language%20Specification.md#indentation) and as such are considered as [Subitems](Language%20Specification.md#items).

  * Cannot have trailing spaces as they might get added for alignment and as such are considered insignificant whitespaces.

  * Cannot be passed as an argument.

* `String`

  * Is a series of Unicode characters quoted by `"`.
    * The exact syntax is specified by the [Syntax Definition](Syntax%20Definition.md#string) file.

  * Can be concatenated with any other `RawString`, `String` or `Text` on the same side of the line that produces a single .NET `String` instance.

* `RawString`

  * Has to start with `@` immediately followed by `'`
    and an optional delimiter which is any legal [Identifier](Language%20Specification.md#identifier)
    continuing with `"` and then the content interpreted exactly as they appear in the source.
    The terminator of the string literal is a `"` immediately followed by the optional delimiter and `'`.

  * Can be concatenated with any other `RawString`, `String` or `Text` on the same side of the line that produces a single .NET `String` instance.

  ### Example:

  ```
  @'foo"<foo bar=" > "></foo>"foo'
  ```

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

    * The exact syntax is specified by the [Syntax Definition](Syntax%20Definition.md#number) file.

  * Can have a fixed size by passing the following constants to the [Pseudo Constructor](Language%20Specification.md#pseudo-constructors):

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

  * Defines a [Pseudo Constructor](Language%20Specification.md#pseudo-constructors) that takes multiple [Sections](Language%20Specification.md#sections) or [Items](Language%20Specification.md#items) as arguments that forms a union.

  * Defines two contextual keywords `@source` and `@target` that relate to the input that was passed through the [Pseudo Constructor](Language%20Specification.md#pseudo-constructors).
    * When an [Item](Language%20Specification.md#items) does not have a target and `@target` is specified it is an error.

  * Cannot be inferred.

## Annotations

An `Annotation` represents an action that the engine should apply to a [Section](Language%20Specification.md#sections) or an [Item](Language%20Specification.md#items).

* Has to start with `@` followed by an [Identifier](Language%20Specification.md#identifier).

* Can be specified at the top of a [Section](Language%20Specification.md#sections) or at the top of an [Anonymous Section](Language%20Specification.md#sections).

* Can be specified at the top of a [Nested Section](Language%20Specification.md#items) before the children of an [Item](Language%20Specification.md#items).

* Can have a [Pseudo Constructor](Language%20Specification.md#pseudo-constructors).

* Can be extended via plugins.

### Example:

```
"(class\s+)"@source : "$1"@target :: Regex(Foo > FooBar), @Transform
```

### Built-in Annotations

* `@Add`

  Adds top-level [Items](Language%20Specification.md#items) and their children to an existing [Section](Language%20Specification.md#sections).

* `@Include`

  Includes the [Items](Language%20Specification.md#items) of a [Section](Language%20Specification.md#sections) within top-level [Items](Language%20Specification.md#items) of another [Section](Language%20Specification.md#sections).

* `@Use`

* `@Transform`

## Syntax

### Comments

A single-line `Comment` starts with `#`.

A multi-line `Comment` starts with `<#` and ends with `#>`.

* `Comment`s are ignored by the syntax.

### Pseudo Constructors

A `Pseudo Constructor` is a construct that allows values to be passed to the engine.

* Has to start with `(` followed by a list of `Value`s separated by `,` and ends with `)`.

* More details can be [found here](Language%20Specification.md#farr-engine-net-core-types).

### Labels

A `Label` must start with `[` followed by letters in the following Unicode categories `Lu`, `Ll`, `Lt`, `Lm`, `Lo` and optional intermediate space (`U+0020`) characters and ends with `]`.

### Squash

A `Squash` is a special syntax that starts with a `Label` followed by a `{` and a list of [String](Language%20Specification.md#built-in-Types) or [RawString](Language%20Specification.md#built-in-Types) using  left-hand side of an [Item](Language%20Specification.md#items) as the key separated by `,` and ends with `}` and is used to reference part of a [Section](Language%20Specification.md#sections).

### Values

A `Value` is used to reference a [Section](Language%20Specification.md#sections) or an [Item](Language%20Specification.md#items) and can be any of the following constructs:

* `Label`
* `Squash`
* [Item](Language%20Specification.md#items)

### Identifiers

An `Identifier` must start with letters in the following Unicode categories `Lu`, `Ll`, `Lt`, `Lm`, `Lo` and optionally continue by numbers in following Unicode categories `Nd` and `Nl`.

### Indentations

An `Indentation` is considered after a `EOL` immediately followed by at least a single space character (`U+0020`) or at least a single `\t` character.

### End Of Line

An `End Of Line` (`EOL`) is considered when the line ends with a sequence of `\r\n\` characters or a single `\n` character.

### End Of File

An `End Of File` (`EOF`) is considered when the file ends with `\0`.

## FARR Engine: .NET Core Types

### Classes

Users can create custom `Type`s and `Annotation`s by deriving from the following .NET classes:

* `FarrType`

* `FarrAnnotation`

### Interfaces

* `IHasPseudoConstructor`

  * When a .NET class that derives from `FarrType` is created and then loaded by the engine,
    it checks for the `IHasPseudoConstructor` interface and then use the `Invoke` method to get the arguments that were passed to the type.