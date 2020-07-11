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

A section has a fixed set of items that are processed as a unit by the engine.

* Can be labeled. A label accepts only letters and intermediate spaces as defined by Unicode `Lu`, `Ll`, `Lt`, `Lm`, `Lo`, `U+0020`.
* Can be decorated with a `Type` or `Annotation`s which will apply to all of the items in the set.
  * When a `Type` and an `Annotation` are specified then the `Type` must precede the `Annotation`'s declaration.
* Is a fixed set therefore during processing items cannot be modified, added or removed.

## Item

An item holds the content which is expressed by a series of Unicode characters
and has a well defined type in the language depending on the shape of the item.

* Can be listed as a single item.
* Can be listed as a key-value pair using the colon symbol `:`.
* Can be listed as a transformation using the greater than symbol `>` which is a syntactic sugar to the key-value pair with the `@Transform` annotation that applies to the item.
* Can be decorated with a `Type` or an `Annotation`s.
  * When a `Type` and an `Annotation` are specified then the `Type` must precede the `Annotation`'s declaration.

## Type

A type can be specified to the left of an `Item` or a `Section` using the double colon syntax `::`
and start with a Unicode character in the following categories `Lu`, `Ll`, `Lt`, `Lm`, `Lo`
that can be followed by `Nd` and `Nl`.

The language provides a dedicated syntax for most types
and can infer the type from the context in most cases so there is no need to specify the type explicitly;
however, sometimes we may want to pass valuable information to the FARR engine,
in this case, we can specify the type explicitly and pass the information to it 
using the pseudo constructor of the type, e.g., `Regex(Foo > FooBar)`.

* Can be applied to a `Section` or an `Item` to pass information to the engine or to enhance the IDE experience.
* Can either take no arguments at all or have multiple arguments.
* Can be extended via plugins.

### Example:

```
"(class\s+)"@source > "$1"@target :: Regex(Foo > FooBar)
```

### Built-in Types

* `String`
  * Is a series of Unicode characters quoted by the ASCII quotation mark symbol `"` as defined by Unicode `U+0022`.
    * The exact syntax is specified by the [Syntax Definition](Syntax%20Definition.cd#String) file. 
  * Is verbatim by default, meaning, anything within the enclosed quotes is escaped except quotation marks.
    * To escaped a quotation mark within a `String` add a preceding quotation mark.
  * Can be concatenated with any other `String` or `Text` on the same side of the line that produces a single .NET `String` instance.
* `Regex`
  * Supports the same syntax and semantics as defined by .NET Regular Expressions.
  * Defines a pseudo constructor that takes multiple `Section`s or `Item`s as arguments that forms a union.
  * Defines two contextual keywords `@source` and `@target` that relate to the input that was passed through the pseudo constructor.
    * When an `Item` does not have a target and `@target` is specified it is an error.
* `Number`
* `File`
* `Text`

## Annotation

An annotation has to start with the at symbol `@`
followed by Unicode characters in the following categories `Lu`, `Ll`, `Lt`, `Lm`, `Lo` that can be followed by `Nd` and `Nl`.

* Can be applied to a `Section` or an `Item` to pass information to the engine.
* Can either take no arguments at all or have multiple arguments.
* Can be extended via plugins.

The difference between a `Type` and an `Annotation` is that
the former defines the way the FARR engine interprets the content
and the latter provides the behaviour or action that applies to the content.

### Example:

```
"(class\s+)"@source : "$1"@target :: Regex(Foo > FooBar), @Transform
```

### Built-in Annotations

* `@Use`
* `@AddTo`
* `@Transform`

## Comments

A single-line comment starts with the hash symbol `#`.

A multi-line comment starts with `<#` and ends with `#>`.

Comments are ignored by the syntax.

## FARR Engine - Core Types

Users can create custom `Type`s and `Annotation`s by deriving from the following .NET classes.

* `FarrType`
* `FarrAnnotation`