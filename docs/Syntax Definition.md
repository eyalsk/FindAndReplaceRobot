# Find And Replace Robot (FARR) - Syntax Definition

```
farr-file
    : section+
    | (section subsection)+
    ;
```

```
subsection
    : (indent section)+
    ;

section
    : annotation* [label newline] set newline
    ;

set
    : (value newline)+ 
    | (map newline)+
    ;
```

```
annotation
    : '@' identifier ['(' annotation-argument-list ')'] newline
    ;

annotation-argument-list
    : annotation-argument (',' annotation-argument)*
    ;

annotation-argument
    : value
    | map
    | label
    ;
```

```
map 
    : value '->' value
    ;

value 
    : value-literal
    | text-character-start text-character+
    ;

label
    : `[` identifier `]`
    ;

identifier
    : identifier-character+
    ;
```

```
value-literal
    : string
    | regex
    | integer
    ;

string
    : '"' string-character+ '"'
    ;

regex
    : '/' ? Any pattern accepted by the .NET regular expression engine ? '/'
    ;

integer
    : nonzerodigit digit*
    ;
```

```
string-character
    : ? Any character except '"' unless escaped with '\' ?
    ;

identifier-character
    : ? Any unicode character in categories Lu, Ll, Lt, Lm, Lo ?
    ;

text-character-start
    : ? text-character and not '[' or '\[' ?
    ;

text-character
    : ? Any character except whitespace, indent and newline ?
    ;
```

```
nonzerodigit
    : "1" ... "9"
    ;

digit
    : "0" ... "9"
    ;
```

```
whitespace
    : ? Any character with Unicode class Zs ?
    | ? Horizontal tab character (U+0009) ?
    | ? Vertical tab character (U+000B) ?
    | ? Form feed character (U+000C) ?
    ;

indent
    : \t
    | U+0009
    ;

newline
    : \n
    | U+000A
    ;
```

```
keyword
    : @source
    | @target
    ;
```