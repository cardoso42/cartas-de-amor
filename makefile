# Caminho para o PlantUML .jar
PLANTUML_JAR = /home/cardoso42/plantuml/plantuml-1.2024.4.jar

# Caminho para a aplicação frontend de testes
FRONTEND_DIR = src/CartasDeAmorFrontTest

# Caminhos para os projetos .NET
BACKEND_DIR = src/CartasDeAmorBack
PRESENTATION_PROJ = $(BACKEND_DIR)/CartasDeAmor.Presentation/CartasDeAmor.Presentation.csproj
INFRAESTRUCTURE_PROJ = $(BACKEND_DIR)/CartasDeAmor.Infrastructure/CartasDeAmor.Infrastructure.csproj

# Arquivos .puml da seção SRS
SRS_PUMLS := $(wildcard docs/srs/diagrams/*.puml)

# Alvo padrão — não faz nada
all:
	@echo "Use an explicit target, e.g.: make srs"

# Geração completa do SRS (diagramas + PDF)
srs: srs-diagrams srs-pdf clean

# Gera os diagramas do SRS
srs-diagrams:
	@echo "Gerando diagramas PlantUML..."
	java -jar $(PLANTUML_JAR) $(SRS_PUMLS)

# Compila o arquivo LaTeX do SRS
srs-pdf:
	@echo "Compilando LaTeX para PDF..."
	cd docs/srs && latexmk -pdf -interaction=nonstopmode main.tex

# Limpa os PNGs gerados em todas as subpastas diagrams
clean:
	@echo "Removendo arquivos auxiliares do LaTeX..."
	cd docs/srs && rm -f *.aux *.log *.out *.toc *.fls *.fdb_latexmk *.synctex.gz *.bbl *.blg

database:
	psql -h localhost -U princess -d love_letter

database-update:
	@echo "Atualizando o banco de dados..."
	dotnet ef database update --project $(INFRAESTRUCTURE_PROJ) --startup-project $(PRESENTATION_PROJ)

front:
	cd $(FRONTEND_DIR) && python3 -m http.server 8080

back:
	dotnet watch run --project $(PRESENTATION_PROJ) --startup-project $(PRESENTATION_PROJ)

migration-add:
	@echo "Adicionando nova migração..."
	@read -p "Digite o nome da migração: " name; \
	if [ -z "$$name" ]; then \
		echo "Nome da migração não pode ser vazio"; \
		exit 1; \
	fi; \
	dotnet ef migrations add $$name --project $(INFRAESTRUCTURE_PROJ) --startup-project $(PRESENTATION_PROJ)

migration: migration-add database-update

undo-last-migration:
	@echo "Desfazendo a última migração..."
	
	@LAST_MIGRATION=$$(dotnet ef migrations list --project $(INFRAESTRUCTURE_PROJ) --startup-project $(PRESENTATION_PROJ) | tail -n 1 | awk '{print $$1}'); \
	if [ -z "$$LAST_MIGRATION" ]; then \
		echo "Nenhuma migração encontrada para desfazer."; \
		exit 1; \
	fi; \
	echo "Última migração: $$LAST_MIGRATION"; \
	\
	echo "Primeiro, revertendo a migração do banco de dados..."; \
	PREV_MIGRATION=$$(dotnet ef migrations list --project $(INFRAESTRUCTURE_PROJ) --startup-project $(PRESENTATION_PROJ) | tail -n 2 | head -n 1 | awk '{print $$1}'); \
	if [ -n "$$PREV_MIGRATION" ] && [ "$$PREV_MIGRATION" != "$$LAST_MIGRATION" ]; then \
		echo "Revertendo para a migração: $$PREV_MIGRATION"; \
		dotnet ef database update "$$PREV_MIGRATION" --project $(INFRAESTRUCTURE_PROJ) --startup-project $(PRESENTATION_PROJ); \
	else \
		echo "Revertendo todas as migrações (não há migração anterior)..."; \
		dotnet ef database update 0 --project $(INFRAESTRUCTURE_PROJ) --startup-project $(PRESENTATION_PROJ); \
	fi; \
	\
	echo "Agora removendo a migração..."; \
	dotnet ef migrations remove --project $(INFRAESTRUCTURE_PROJ) --startup-project $(PRESENTATION_PROJ)
