# Contributing to SeedLang

Thanks for taking the time to contribute!

## Design decisions

There are lots of trade-offs to made when building a new programming
environment. We will document every significant design decision in the `design`
dir. Please create a new issue labeled with `design` if you find anything
undocumented or propose an update for an existing design.

## Submitting changes

For the main branch, all commits must be submitted via a pull request with at
least one approving reviews from the core team.

For the developers who have write-access to the repository, please consider the
following branch naming conventions before submitting a pull request:

* Release branches: `release_<version>`
* Personal working branches:
  * `wip_<your-github-id>`, or
  * `wip_<your-github-id>_<task-desc>`, or
  * `wip_<your-github-id>_<sequence_no>`, or
* Experimental branches:
  * `exp_<your-github-id>`, or
  * `exp_<your-github-id>_<sequence_no>`
* Temporary bugfix branch: `bugfix_<issue-id>`
* Temporary hotfix branch: `hotfix_<issue-id>`
* Temporary feature branch: `feature_<issue-id>`

Please follow [How to Write a Git Commit
Message](https://chris.beams.io/posts/git-commit/) when writing your commit
messages whenever possible.

## Coding conventions

We follow the [Google Style Guides](https://google.github.io/styleguide/) for
each programming language used in the project.

For documentations, please follow the [Google documentation
guide](https://google.github.io/styleguide/docguide/) whenever possible.

Directory names and filenames in the repo should be all lowercase, with
underscores to separate words, unless the coding style of the source language
prefers or requires other styles. For example, directory names and filenames of
C# code should be PascalCase.

### Exceptions

* For Markdown docs, the default
  [markdownlint](https://github.com/markdownlint/markdownlint) rule set
  overrides the [Google Markdown style
  guilde](https://google.github.io/styleguide/docguide/style.html) if the two
  conflict.
