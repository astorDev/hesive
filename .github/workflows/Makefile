action:
	mkdir -p ./temp
	cp module-name-nuget.yml ./temp/module-name-nuget.yml
	replace --all-cases --folder ./temp module-name $(MODULE)
	replace --all-cases --folder ./temp your_path $(PATH)
	cp ./temp/* .
	rm -rf ./temp