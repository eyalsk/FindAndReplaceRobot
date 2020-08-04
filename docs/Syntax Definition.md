# Find And Replace Robot (FARR) - Syntax Definition

**WORK IN PROGRESS**

## FARR File

```
farr_file
    : section*
    ;
```

## Sections

```
section
    : named_section
    | anonymous_section
    ;

anonymous_section
    : annotation_block? item_block eol
    | annotation_block? '[]' (space+ type_provider)? eol item_block eol
    ;

named_section
    : annotation_block? label (space+ type_provider)? eol item_block eol
    ;
```

## Items

```
item_block
    : item item*
    | item indentation annotation_block? item_block
    ;

item
    : item_transformation
    | item_pair
    | item_single
    ;

item_transformation
    : item_value space+ '>' space+ item_value (space+ type_provider)? eol
    ;

item_pair
    : item_value space+ ':' space+ item_value (space+ type_provider)? eol
    ;

item_single
    : item_value (space+ type_provider)? eol
    ;

item_value
    : structured_value
    | text
    ;
```

## Types

```
type_provider
    : '::' space+ type space* (',' space* type space*)*
    ;

type
    : identifier pseudo_constructor?
    ;
```

## Annotations

```
annotation_block
    : annotation eol
    | annotation_block+
    ;

annotation
    : '@' identifier pseudo_constructor?
    ;
```

## Pseudo Constructors

```
pseudo_constructor
    : '(' argument_list? ')'
    ;

argument_list
    : argument space* (',' space* argument space*)*
    ;

argument
    : structured_value
    | label
    | selection
    ;
```

## Literals

```
structured_value
    : string
    | raw_string
    | number
    | regex
    ;

regex
    : '"/' '< Any pattern accepted by the .NET Regular Expression engine except '/"' unless it's escaped with `\` >' '/"'
    ;

text
    : ^whitespace \X+ ^whitespace
    ;

selection
    : label '{' space* string space* (',' space* string)* space* '}'
    | label '{' space* raw_string space* (',' space* raw_string)* space* '}'
    ;

label
    : '[' identifier ']'
    ;
```

## Numbers

```
binary
    : '0b' digit_separator* [0-1]+ (digit_separator* [0-1]+)*
    ;

hex
    : '0x' digit_separator* [0-9a-fA-F]+ (digit_separator* [0-9a-fA-F]+)*
    ;

unsigned_number
    : number unsigned_number_suffix?
    ;

signed_number
    : '-'? number signed_number_suffix?
    ;

number
    : digit+ digit_separator* digit+ ('.' digit+ digit_separator* digit+)?
    ;

unsigned_number_suffix
    : u8
    | u16
    | u32
    | u64
    ;

signed_number_suffix
    : i8
    | i16
    | i32
    | i64
    | f32
    | f64
    ;

digit
    : [0-9]
    ;

digit_separator
    : space
    | '_'
    ;
```

## Strings

```
raw_string
    : '"` (identifier '`')? \X+ ('`' identifier)? `"'
    ;

string
    : '"' string_character+ '"'
    ;

string_character
    : [^"]
    | '\"'
    | '\`'
    ;
```

## Keywords

```
keyword
    : '@' identifier
    ;
```

## Identifiers

```
identifier
    : identifier_character+ \p{N}* identifier_character*
    ;

identifier_character
    : \p{L}
    ;
```

## Comments

```
singleline_comment
    : '#' \X+
    ;

multiline_comment
    : '<#' \X+ '#>'
    ;
```

## Invisible Characters

```
whitespace
    : \s
    | \p{zs}
    ;

indentation
    : space
    ;

space
    : [\t ]
    | \u0020
    | \u2009
    | \u202F
    | \u0009
    | \u000B
    ;

eol
    : \n
    | \r\n
    | \u000A
    ;

eof
    : \0
    ;
```