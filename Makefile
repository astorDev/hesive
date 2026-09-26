pr:
	test "$$(git default-branch)" == "$$(git branch --show-current)" || throw "Current branch is NOT default ($$(git branch --show-current)). Full PR is only possible from the default branch."
	git switch --create $(BRANCH)
	git save "$(TITLE)"
	gh pr create --title "$(TITLE)" --body "" || true
	gh pr view --web

lib:
	dotnet new lib --output $(MODULE)/lib
	copaster-magic $(MODULE)/lib --project=Hesive --module=$(MODULE)