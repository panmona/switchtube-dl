## Contributing to switchtube-dl

:+1::tada: First off, thanks for taking the time to contribute! :tada::+1:

#### **Run Locally**

##### Prerequisites

- .net6 SDK needs to be installed

##### Steps

Clone the repository

```bash
git clone git@github.com:panmona/switchtube-dl.git
```

Go to the project directory

```bash
cd switchtube-dl
```

Install the necessary tools

```bash
dotnet tool restore
```

Test the CLI

```bash
cd src/SwitchTubeDl
dotnet run -- {your args}
```

Run tests

```bash
cd src/Tests
dotnet run
```

#### **Did you find a bug?**

* **Ensure the bug was not already reported** by searching on GitHub
  under [Issues](https://github.com/panmau/switchtube-dl/issues).

* If you're unable to find an open issue addressing the
  problem, [open a new one](https://github.com/panmau/switchtube-dl/issues/new). Be sure to include a **title and clear
  description** and as much relevant information as possible demonstrating the expected behavior that is not occurring.

* Use the bug report template to create the issue.

#### **Did you write a patch that fixes a bug?**

If you've gone the extra mile and have a patch that fixes the issue, you should submit a Pull Request!

* Please follow our Coding Conventions
* Fork the repo on Github.
* Create a feature branch from where you want to base your work.
* Use "fix", "add", "change" instead of "fixed", "added", "changed" in your commit messages.
* The first letter in your commit message should be upper-case
* Push to your fork and submit a pull request.
* Ensure the PR description clearly describes the problem and solution. Include the relevant issue number if applicable.

#### **Do you intend to add a new feature or change an existing one?**

* Suggest your change in an issue before you start writing code.

#### **Coding Conventions**

All source code is formatted according to the [Styleguide](https://github.com/G-Research/fsharp-formatting-conventions), with the exception of us using the Stroustroup Style.
You can format it that way by using this command:

```bash
dotnet fantomas src -r
```

Ensure that the fantomas tool is up to date:

```bash
dotnet tool restore
```
