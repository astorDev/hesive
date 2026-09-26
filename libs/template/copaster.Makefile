in-output:
	replace --all-cases ProjectName $(PROJECT)
	replace --all-cases ModuleName $(MODULE)
	rm -rf copaster.Makefile

in-caller:
	dotnet sln add $(OUTPUT) --in-root