Language Specification
----------------------

FARR stands for Find and Replace Robot  
and is designed to do a mass structural find and replace.

FARR treats the source and target as objects as opposed to a plain text  
where each object has a well defined type such as `string`, `regex`, `text`, etc...  

A FARR file ends with the extension `.farr` and contains the input  
to the FARR engine.

A simple FARR file looks like the following:
```
[SectionName]
Item -> item
```

The main goal of FARR is to be used as a Find and Replace tool;  
however, the language and the engine is designed to be more general than that,  
meaning, it's possible to use the language independently from the tool to fit different use cases.  

## Section

A section has the following syntax `[SectionName]`  
and is a fixed set of objects that is mapped or processed as single unit by the engine.  

* It has a name.  
* It can be decorated with annotations that dictate how to treat and process the items in the set.  
* It is a fixed set therefore during processing items cannot be added or removed.  
* It can be referenced by its name anywhere in the file.