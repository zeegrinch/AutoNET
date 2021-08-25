# A CLI utility for direct user-interaction with .NET Objects   

<br /><br />

## Requirements

Implement a command console for changing settings on a particular object.  The command console should allow you to enter a string and will return the response (very similar to a terminal session).  The commands are as follows:

```
SET propertyname=newvalue 
```
will change the target object’s member named “propertyname” to have a value equal to “newvalue”.  If the input value is incompatible (i.e. an int being set to a string), print out an appropriate error message.

```
GET propertyname
```
 will print out the current value of the target object’s member named “propertyname”. 
 
 ```
 GET * 
 ```
 will print out a list of all target object members and their current values.

The system should be _extensible_ for _future commands_ and should accept an arbitrary object, such that another developer could insert another object into the system and rely on the command console to get and set the properties correctly.

<sub><sup>_Note: this is a contrived, artificial software product designed to demonstrate concepts and clean-coding principles. Arguably one of the  most efective way of automating and interacting 
with CLR objects is using PowerShell. The requirements are also a bit ambiguous on purpse: to allow for ellaboration and a creative yet realistic design & implementation._ </sup></sub>


<br /><br />

## Requirements-refined

This section captures a more detailed description of the requirements, as understood by the implementer; just an attempt to clarify  terminology and validate assumptions.

* __Object__: the running instance of a class (a runtime-time artifact)

    This means that the utility has to allow for specifying a given type (or C# class) that the user wants to create an instance of and interact with.
    _Particular object_ therefore, refers to the current instance the user has somehow selected to interact with (see below). 

    The requirement uses the term _settings_ to refer to attributes of an object that can be changed: in more technical terms, this means  R/W properties and public fields.

* __Session__: we will use this term to refer to the current user-CLI utility interaction: it starts with the user launching the __TestRig.exe__ program and ends when the application exists (Ctrl+C or issues the _quit_ command).

    When the utility is launched it scans the [./Source] sub-folder  (or a folder sepcified in a config parameter using an absolute path) for C# code (*.cs) files. 
    Then, it attemps to compile each file and subsequently parse the resulting assemblies using reflection APIs and build a type map. 
    The user must select a specific type, from the available list- and thus instruct the runtime to create an instance of it- to be able to 'interact' with an instance (object) of that type.

* __Context__

    During a particular session there is __only one instance__ of a class that the user interacts with: __the current object__. We refer to that as the _current context_.
    All the GET/SET/INVOKE commands that the user issues are going to target the current object. The name of the type (class) of the current object is always displayed as part of the prompt in the CLI.      

<br /><br />

## Engineering Goals

1. Simplicity & Robustness: we aimed for a simple implementation, as complex as needed,  without unnecessary frills or sofistication
2. Clean-Code: the author intended to align as much as possible to SOLID principles, especially (in this case) to the Single-Responsibiliy   
3. Observability: provide good instrumentation and visibility into the system at runtime (to allow for troubleshooting and other implementaion adjustements)
4. Security:



