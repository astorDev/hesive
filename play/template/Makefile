TEST ?= main

run:
	dotnet run

kill:
	kill `lsof -t -i:5208`

yac:
	httpyac send $(TEST).http --all

play:
	bash -c -i 'make run & sleep 5 && make yac && kill -INT %1'

apply-v:
	cp Versions/$(V)/Program.cs Program.cs

stash-v:
	mkdir -p Versions/$(V) && cp Program.cs Versions/$(V)/Program.cs

play-with-v:
	make apply-v
	make play

test:
	dotnet test --filter FullyQualifiedName~Template.Playground.$(NAME) --logger "console;verbosity=detailed"