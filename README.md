# Readme
This is a simple light weight Auto-Mocking container for TypeMock. Note you must have TypeMock installed to be able to build and use.

# Usage
```cs
var mocker = new Automocker();
var sut = mocker.CreateSut<MyClass>();
var dependency1 = mocker.GetMock<IDependency1>();
var dependency2 = mocker.GetMock<IDependency2>();
```