include .env

update_web:
	./update_version.sh ./Client/version
	s3cmd put Client/style.css s3://$(BUCKET_NAME)  --add-header='content-type':'text/css'  
	s3cmd put Client/index.html s3://$(BUCKET_NAME)
	s3cmd put Client/script.js s3://$(BUCKET_NAME) -m 'text/javascript'
	s3cmd put Client/version s3://$(BUCKET_NAME)

update_server_ver:
	./update_version.sh ./Server/Messages.Api/version

update_server_image:
	 docker build -f Server/Dockerfile ./Server -t cr.yandex/$(YC_IMAGE_REGISTRY_ID)/$(CONTAINER_NAME)
	 docker push cr.yandex/$(YC_IMAGE_REGISTRY_ID)/$(CONTAINER_NAME)

deploy_server:
	yc serverless container revision deploy \
	    --container-name $(SERVERLESS_CONTAINER_NAME) \
	    --image 'cr.yandex/$(YC_IMAGE_REGISTRY_ID)/$(SERVERLESS_CONTAINER_NAME):latest' \
	    --service-account-id $(SERVICE_ACCOUNT_ID)  \
	    --environment='$(shell tr -s '\r\n' ',' < Server/Messages.Api/.env | cut -c 4-)'\
	    --execution-timeout 30s \
		--min-instances $(PREP_INSTANCES) \

update_server: update_server_ver update_server_image deploy_server

create_gw_spec:
	$(shell sed "s/SERVERLESS_CONTAINER_ID/${SERVERLESS_CONTAINER_ID}/;s/SERVICE_ACCOUNT_ID/${SERVICE_ACCOUNT_ID}/" api-gw.yaml.example > api-gw.yaml)

create_gw: create_gw_spec
	yc serverless api-gateway create --name $(SERVERLESS_CONTAINER_NAME) --spec api-gw.yaml
	yc serverless api-gateway get $$(yc serverless api-gateway list | grep $(SERVERLESS_CONTAINER_NAME) | awk -F'|' '{print $$2}' | xargs)